package no.hesa.positionlibrary;

import android.content.BroadcastReceiver;
import android.content.Context;
import android.content.Intent;
import android.content.IntentFilter;
import android.net.wifi.ScanResult;
import android.net.wifi.WifiInfo;
import android.net.wifi.WifiManager;

import org.apache.commons.math3.fitting.leastsquares.LeastSquaresOptimizer;
import org.apache.commons.math3.fitting.leastsquares.LevenbergMarquardtOptimizer;
import org.apache.commons.math3.linear.RealVector;

import java.util.Arrays;
import java.util.Collection;
import java.util.HashMap;
import java.util.List;
import java.util.Map;
import java.util.Set;
import java.util.Timer;
import java.util.TimerTask;
import java.util.TreeMap;

/**
 * A class that takes care of positioning using wifi access-points
 */

public class WifiPosition {
    private List<ScanResult> scanResults = null;
    private static final double radius = 6371*1000;
    private final BroadcastReceiver wifiReceiver = new BroadcastReceiver() {
        @Override
        public void onReceive(Context c, Intent intent) {
            if (intent.getAction().equals(WifiManager.SCAN_RESULTS_AVAILABLE_ACTION)) {
                WifiManager wifiManager = (WifiManager) c.getSystemService(Context.WIFI_SERVICE);
                scanResults = wifiManager.getScanResults();
                calculateDistances(c);
                //wifiManager.startScan();
            }
        }
    };

    public void registerBroadcast(Context c) {
        c.registerReceiver(wifiReceiver, new IntentFilter(WifiManager.SCAN_RESULTS_AVAILABLE_ACTION));
    }
    public void unRegisterBroadcast(Context c) {
        c.unregisterReceiver(wifiReceiver);
    }

    public void calculateDistances(Context c) {
        if (scanResults != null) {
            HashMap<String, Double> wifiPointsScanInf = new HashMap<>(scanResults.size());//MAC addresses to Wi-Fi access points with corresponding distances
            for (int i = 0; i < scanResults.size(); i++) {
                if(scanResults.get(i).frequency > 5000 && scanResults.get(i).SSID.equals("eduroam")) {
                    wifiPointsScanInf.put(scanResults.get(i).BSSID, distanceToAccessPoint(scanResults.get(i).level, scanResults.get(i).frequency));
                }
            }

            StringBuilder outputBuilder = new StringBuilder();

            HashMap<Double, Point> wifiPointsLocationInf = new HashMap<Double, Point>(matchMACtoGeo(wifiPointsScanInf));//Distances to Wi-Fi access points with corresponding geo coordinates
            Map<Double, Point> sortedWifiPointsLocationInf = new TreeMap<Double, Point>(wifiPointsLocationInf);

            //Checking number of access points. If there are more than three, than keep only closest three
            if(sortedWifiPointsLocationInf.size() > 3){
                Double [] keys = (Double[]) sortedWifiPointsLocationInf.keySet().toArray(new Double[sortedWifiPointsLocationInf.size()]);
                while (sortedWifiPointsLocationInf.size() > 3){
                    sortedWifiPointsLocationInf.remove(keys[sortedWifiPointsLocationInf.size() - 1]);
                }
            }

            //If there are three available Wi-Fi access points with known coordinates, calculating position
            double[] calculatedPosition = null;
            if(sortedWifiPointsLocationInf.size() == 3) {
                calculatedPosition = calculatePosition(sortedWifiPointsLocationInf);
            }

            Intent intent = new Intent();
            intent.setAction("no.hesa.positionlibrary.Output");

            if(calculatedPosition != null)
                intent.putExtra("position", calculatedPosition);
                //outputBuilder.append("Your position: " + Double.toString(calculatedPosition[0]) + " " + Double.toString(calculatedPosition[1]) + "\n");
            else {
                double[] noPosition = {0,0};
                intent.putExtra("position", noPosition);
                //outputBuilder.append("Cant find your position :(" + "\n");
            }

            //Intent intent = new Intent();
            //intent.setAction("no.hesa.positionlibrary.Output");
            //intent.putExtra("DistanceOutput", outputBuilder.toString());
            //intent.putExtra("lat.", calculatedPosition[0]);
            //intent.putExtra("lng.", calculatedPosition[1]);
            //intent.putExtra("position", calculatedPosition);
            c.sendBroadcast(intent);
        }
    }

    /**
     * Calculate the distance to one access point given RSSI in db and frequency in MHz
     * @param levelInDb RSSI (received signal strength indication) in decibels
     * @param freqInMHz Frequency in MHz
     * @return The distance to the access-point in meters
     */
    private double distanceToAccessPoint(double levelInDb, double freqInMHz)    {
        double exp = (27.55 - (20 * Math.log10(freqInMHz)) + Math.abs(levelInDb)) / 20.0;
        return Math.pow(10.0, exp);
    }

    private static double radiansFromDeg(double degrees) {
        return degrees * (Math.PI / 180);
    }

    private static double degFromRadians(double radians) {
        return radians * (180 / Math.PI);
    }

    public static  double[] calculateCoordinates(double[] from, double distanceKm, double bearingDegrees) {
        double distanceRadians = (double) distanceKm/6371;
        double bearingRadians = radiansFromDeg(bearingDegrees);
        double fromLatRadians = radiansFromDeg(from[0]);
        double fromLonRadians = radiansFromDeg(from[1]);
        double toLatRadians = Math.asin(Math.sin(fromLatRadians) * Math.cos(distanceRadians) + Math.cos(fromLatRadians) * Math.sin(distanceRadians) * Math.cos(bearingRadians));
        double toLonRadians = fromLonRadians + Math.atan2(Math.sin(bearingRadians) * Math.sin(distanceRadians) * Math.cos(fromLatRadians),Math.cos(distanceRadians)-Math.sin(fromLatRadians) * Math.sin(toLatRadians));

        toLonRadians = ((toLonRadians + 3*Math.PI) % (2*Math.PI)) - Math.PI;
        return new double[]{degFromRadians(toLatRadians),degFromRadians(toLonRadians)};
    }
    /**
     * Calculate from geo coordinates too cartesian coordinates
     */
    public static double[] convertFromGeo(double[] coordinates) {
        double x_cartesian = radius * Math.cos(coordinates[0]*Math.PI/180) * Math.cos(coordinates[1]*Math.PI/180);
        double y_cartesian = radius * Math.cos(coordinates[0]*Math.PI/180) * Math.sin(coordinates[1] * Math.PI / 180);
        double z_cartesian = radius * Math.sin(coordinates[0] * Math.PI / 180);
        return new double[] {x_cartesian,y_cartesian,z_cartesian};
    }

    /**
     * Calculate from cartesian coordinates to geo coordinates
     */
    public static double[] convertToGeo(double[] coordinates) {
        double x_deg = (180/Math.PI)*Math.asin(coordinates[2] / radius);
        double y_deg = (180/Math.PI)*Math.atan2(coordinates[1], coordinates[0]);
        return new double[] {x_deg,y_deg};
    }

    /**
     * Match distances to available Wi-Fi access points with corresponding geo coordinates against MAC address
     * @param wifiPointsScanInf list of MAC addresses to available Wi-Fi access points with corresponding distances
     * @return list of distances to available Wi-Fi access points with corresponding geo coordinates
     */
    public HashMap<Double, Point> matchMACtoGeo(Map<String, Double> wifiPointsScanInf){
        HashMap<Double, Point> wifiPointsLocationInf = new HashMap<Double, Point>(wifiPointsScanInf.size());
        Double [] keys = (Double[]) wifiPointsScanInf.values().toArray(new Double[wifiPointsScanInf.size()]);
        String [] values = (String[]) wifiPointsScanInf.keySet().toArray(new String[wifiPointsScanInf.size()]);

        //List of MAC addresses to known Wi-Fi access point with corresponding geo coordinates
        //frequency: 2.4
        TreeMap<String, Point> wifiPointsMacGeo = new TreeMap<String, Point>();
        /*wifiPointsMacGeo.put("ec:bd:1d:8b:8d:b0", new Point(68.43611583610615, 17.43369428932667));
        wifiPointsMacGeo.put("ec:bd:1d:6b:8d:40", new Point(68.43606297172468, 17.433767840266228));
        wifiPointsMacGeo.put("ec:bd:1d:6b:8e:70", new Point(68.43617239716085, 17.433753423392773));
        wifiPointsMacGeo.put("ec:bd:1d:88:85:70", new Point(68.43623622842107, 17.43380941450596));
        wifiPointsMacGeo.put("34:ab:4e:fc:5f:40", new Point(68.4362602575168, 17.43396732956171));
        wifiPointsMacGeo.put("5c:83:8f:34:f9:59", new Point(68.43618089979086, 17.434068247675896));*/

        //frequency: 5.0
        wifiPointsMacGeo.put("ec:bd:1d:8b:8d:bf", new Point(68.43611583610615, 17.43369428932667));
        wifiPointsMacGeo.put("ec:bd:1d:6b:8d:4f", new Point(68.43606297172468, 17.433767840266228));
        wifiPointsMacGeo.put("ec:bd:1d:6b:8e:7f", new Point(68.43617239716085, 17.433753423392773));
        wifiPointsMacGeo.put("ec:bd:1d:88:85:7f", new Point(68.43623622842107, 17.43380941450596));
        wifiPointsMacGeo.put("34:ab:4e:fc:5f:4f", new Point(68.4362602575168, 17.43396732956171));
        wifiPointsMacGeo.put("5c:83:8f:34:f9:5f", new Point(68.43618089979086, 17.434068247675896));

        //Checking MAC addresses of available Wi-Fi access points against list of MAC addresses to known Wi-Fi access points
        for(int i = 0; i < wifiPointsScanInf.size(); i++){
            //If there is a match, putting distance with corresponding geo coordinates in the wifiPointsLocationInf
            if(wifiPointsMacGeo.containsKey(values[i]))
                wifiPointsLocationInf.put(keys[i], wifiPointsMacGeo.get(values[i]));
        }
        return wifiPointsLocationInf;
    }

    /**
     * Calculate current geo coordinates
     * @param wifiPointsLocationInf list of distances to three closest Wi-Fi access points with corresponding geo coordinates
     * @return current geo coordinates
     */
    public double[] calculatePosition(Map<Double, Point> wifiPointsLocationInf){
        double[] calculatedPosition = null;
        Point[] degrees = (Point[]) wifiPointsLocationInf.values().toArray(new Point[wifiPointsLocationInf.size()]);

        double[][] cartesian = new double[wifiPointsLocationInf.size()][3];

        for (int i = 0; i < wifiPointsLocationInf.size(); i++) {
            cartesian[i] = WifiPosition.convertFromGeo(new double[]{degrees[i].getLatitude(), degrees[i].getLongitude()});
        }
        Double[] distances = (Double[]) wifiPointsLocationInf.keySet().toArray(new Double[wifiPointsLocationInf.size()]);
        TrilaterationFunction trilaterationFunction = new TrilaterationFunction(cartesian,distances);
        LinearLeastSquaresSolver lSolver = new LinearLeastSquaresSolver(trilaterationFunction);
        NonLinearLeastSquaresSolver nlSolver = new NonLinearLeastSquaresSolver(trilaterationFunction, new LevenbergMarquardtOptimizer());
        RealVector x = lSolver.solve();
        LeastSquaresOptimizer.Optimum optimum = nlSolver.solve();

        calculatedPosition = optimum.getPoint().toArray();
        calculatedPosition = WifiPosition.convertToGeo(calculatedPosition);

        return calculatedPosition;
    }
}
