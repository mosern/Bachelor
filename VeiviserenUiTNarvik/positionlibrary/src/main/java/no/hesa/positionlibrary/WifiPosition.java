package no.hesa.positionlibrary;

import android.content.BroadcastReceiver;
import android.content.Context;
import android.content.Intent;
import android.content.IntentFilter;
import android.net.wifi.ScanResult;
import android.net.wifi.WifiManager;

import java.util.Arrays;
import java.util.List;
import java.util.Timer;
import java.util.TimerTask;

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
            double[] distances = new double[scanResults.size()];
            for (int i = 0; i < scanResults.size(); i++) {
                distances[i] = distanceToAccessPoint(scanResults.get(i).level, scanResults.get(i).frequency);
            }

            Arrays.sort(distances);

            StringBuilder outputBuilder = new StringBuilder();

            //Checking number of access points
            int length = 0;
            if(distances.length > 3)
                length = 3;
            else
                length = distances.length;

            for (int i = 0; i < length; i++) {
                outputBuilder.append("Distance to access point: " + distances[i] + "\n");
            }

            Intent intent = new Intent();
            intent.setAction("no.hesa.positionlibrary.Output");
            intent.putExtra("DistanceOutput", outputBuilder.toString());
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

    /**
     * Calculate from geo coordinates too cartesian coordinates
     */
    private double[] convertFromGeo(double[] coordinates) {
        double x_cartesian = radius * Math.cos(coordinates[0]*Math.PI/180) * Math.cos(coordinates[1]*Math.PI/180);
        double y_cartesian = radius * Math.cos(coordinates[0]*Math.PI/180) * Math.sin(coordinates[1] * Math.PI / 180);
        double z_cartesian = radius * Math.sin(coordinates[0]*Math.PI/180);
        return new double[] {x_cartesian,y_cartesian,z_cartesian};
    }

    /**
     * Calculate from cartesian coordinates to geo coordinates
     */
    private double[] convertToGeo(double[] coordinates) {
        double x_deg = (180/Math.PI)*Math.asin(coordinates[2] / radius);
        double y_deg = (180/Math.PI)*Math.atan2(coordinates[1],coordinates[0]);
        return new double[] {x_deg,y_deg};
    }

}
