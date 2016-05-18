package no.hesa.positionlibrary;

import android.annotation.TargetApi;
import android.content.BroadcastReceiver;
import android.content.Context;
import android.content.Intent;
import android.content.IntentFilter;
import android.hardware.Sensor;
import android.hardware.SensorEvent;
import android.hardware.SensorEventListener;
import android.hardware.SensorManager;
import android.location.Location;
import android.net.wifi.ScanResult;
import android.net.wifi.WifiManager;
import android.os.Build;
import android.os.Handler;

import org.apache.commons.math3.fitting.leastsquares.LeastSquaresOptimizer;
import org.apache.commons.math3.fitting.leastsquares.LevenbergMarquardtOptimizer;
import org.apache.commons.math3.linear.RealVector;
import org.json.JSONArray;
import org.json.JSONException;
import org.json.JSONObject;

import java.util.ArrayList;
import java.util.Arrays;
import java.util.Collections;
import java.util.HashMap;
import java.util.LinkedList;
import java.util.List;
import java.util.Map;
import java.util.Timer;
import java.util.TimerTask;
import java.util.TreeMap;

import no.hesa.positionlibrary.api.ActionInterface;
import no.hesa.positionlibrary.api.Api;
import no.hesa.positionlibrary.dijkstra.DijkstraAlgorithm;
import no.hesa.positionlibrary.dijkstra.exception.PathNotFoundException;
import no.hesa.positionlibrary.dijkstra.model.Edge;
import no.hesa.positionlibrary.dijkstra.model.Graph;
import no.hesa.positionlibrary.dijkstra.model.Vertex;
import no.hesa.positionlibrary.trillateration.LinearLeastSquaresSolver;
import no.hesa.positionlibrary.trillateration.NonLinearLeastSquaresSolver;
import no.hesa.positionlibrary.trillateration.TrilaterationFunction;

/**
 * A class that takes care of positioning using wifi access-points
 */

@TargetApi(Build.VERSION_CODES.JELLY_BEAN_MR2)
public class WifiPosition implements ActionInterface {
    private List<ScanResult> scanResults = null;
    private static final double radius = 6371 * 1000; //earth radius
    private int counter = 1; //amount of times Wi-Fi scan was run
    private final BroadcastReceiver wifiReceiver = new BroadcastReceiver() {
        @Override
        public void onReceive(Context c, Intent intent) {
            if (intent.getAction().equals(WifiManager.SCAN_RESULTS_AVAILABLE_ACTION)) {
                WifiManager wifiManager = (WifiManager) c.getSystemService(Context.WIFI_SERVICE);
                scanResults = wifiManager.getScanResults();
                calculateDistances(c);
                //If this was first time that code was run, set up TimerTask to make WI-Fi scans every 5 s.
                if (timer == null) {
                    timer = new Timer();
                    initializeTimerTask(wifiManager);
                    timer.schedule(timerTask, 1000, 500);
                    counter++;
                } else {
                    //If not...
                    //sensorUpdateTrigger = false;
                    counter++;
                }
            }
        }
    };
    private Timer timer;
    private TimerTask timerTask;
    final Handler handler = new Handler();
    private HashMap<Double, Point> wifiPointsLocationInfList = new HashMap<Double, Point>(); //list of available Wi-Fi access points with corresponding distances from several scans
    private SensorManager sensorManager;
    private Sensor sensor;
    private SensorEventListener sensorEventListener = new SensorEventListener() {
        @Override
        public void onSensorChanged(final SensorEvent event) {
            Sensor sensor = event.sensor;

            if (sensor.getType() == Sensor.TYPE_ACCELEROMETER) {
                //If it is the fist time onSensorChanged event is run, read the start data on acceleration
                /*if(firstSensorChange){
                    new CountDownTimer(4000, 1000) {
                        public void onFinish() {
                            // When timer is finished
                            xStartValue = event.values[0];
                            yStartValue = event.values[1];
                        }

                        public void onTick(long millisUntilFinished) {

                        }
                    }.start();
                    firstSensorChange = false;
                }
                else{//If not
                    long curentTime = System.currentTimeMillis();
                    long timeGone = curentTime - lastUpdateTime;
                    //Check if more than 2.3 seconds are gone since last detected movement and it is not first detected movement
                    if(lastUpdateTime != 0 && (timeGone >= 2300)){
                        //Check if it was significant enough movement
                        if((Math.abs(event.values[0]) > (xStartValue + 2)) || (Math.abs(event.values[1]) > (yStartValue + 2))){
                            moving = true;
                            lastUpdateTime = curentTime;
                        }
                    }
                    else if(lastUpdateTime == 0)
                        lastUpdateTime = curentTime;
                }*/
                long curentTime = System.currentTimeMillis();
                long timeGone = curentTime - lastUpdateTime;
                //Check if more than 2.3 seconds are gone since last detected movement and it is not first detected movement
                if (lastUpdateTime != 0 && (timeGone >= 2300)) {
                    //Check if it was significant enough movement
                    if ((Math.abs(event.values[0]) > (xStartValue + 2)) || (Math.abs(event.values[1]) > (yStartValue + 2))) {
                        moving = true;
                        lastUpdateTime = curentTime;
                    }
                } else if (lastUpdateTime == 0)
                    lastUpdateTime = curentTime;
            }
        }

        @Override
        public void onAccuracyChanged(Sensor sensor, int accuracy) {

        }
    };
    private long lastUpdateTime = 0; //last time there was detected movement of device
    //private boolean sensorUpdateTrigger = false;
    private boolean moving = false; //Variable that shows if device is moving (true) or not (false)
    private boolean firstSensorChange = true; //Variable that shows if it is first time sensor data was changed
    private double xStartValue = 0; //acceleration on x-axis at the time of first onSensorChanged run
    private double yStartValue = 0; //acceleration on y-axis at the time of first onSensorChanged run
    TreeMap<String, Point> wifiPointsMacGeo = new TreeMap<String, Point>(); //list of MAC addresses to known Wi-Fi access point with corresponding geo coordinates

    private DijkstraAlgorithm da;
    private Graph graph;
    private List<Edge> model;
    private List<Point> allPathPoints= new ArrayList<Point>();
    private Point lastSentPosition;

    /**
     * Initialize TimerTask to set up continuous scannig for available Wi-Fi access points
     * @param wifiManager
     */
    public void initializeTimerTask(final WifiManager wifiManager) {
        timerTask = new TimerTask() {
            public void run() {
                handler.post(new Runnable() {
                    public void run() {
                        wifiManager.startScan();
                    }
                });
            }
        };
    }

    /**
     *
     * @param c
     */
    public void registerBroadcast(Context c){
        c.registerReceiver(wifiReceiver, new IntentFilter(WifiManager.SCAN_RESULTS_AVAILABLE_ACTION));
        WifiManager wifiManager = (WifiManager) c.getSystemService(Context.WIFI_SERVICE);
        wifiManager.startScan();
        //Register listener to detect if device changed position
        sensorManager = (SensorManager) c.getSystemService(c.SENSOR_SERVICE);
        sensor = sensorManager.getDefaultSensor(Sensor.TYPE_ACCELEROMETER);
        sensorManager.registerListener(sensorEventListener, sensor, SensorManager.SENSOR_DELAY_FASTEST);

        Api api = new Api(this);
        //Load list of all mapped wi-fi access points
        api.allAccessPoints();
        //api.allPathPoints(); //TODO remove this
    }

    /**
     *
     * @param c
     */
    public void unRegisterBroadcast(Context c) {
        c.unregisterReceiver(wifiReceiver);
        //sensorManager.unregisterListener(sensorEventListener);
    }

    /**
     *
     * @param c
     */
    public void calculateDistances(Context c) {
        if (scanResults != null) {
            HashMap<String, Double> wifiPointsScanInf = new HashMap<>(scanResults.size());//MAC addresses to Wi-Fi access points with corresponding distances
            for (int i = 0; i < scanResults.size(); i++) {
                if (scanResults.get(i).frequency > 5000 && scanResults.get(i).SSID.equals("eduroam")) {
                    wifiPointsScanInf.put(scanResults.get(i).BSSID, distanceToAccessPoint(scanResults.get(i).level, scanResults.get(i).frequency));
                }
            }

            HashMap<Double, Point> wifiPointsLocationInf = new HashMap<Double, Point>(matchMACtoGeo(wifiPointsScanInf));//Distances to Wi-Fi access points with corresponding geo coordinates

            //Checking if wifiPointsLocationInfList is empty
            if (wifiPointsLocationInfList.size() == 0) {
                //If yes, than it means that just one out of four Wi-Fi scans was run
                //and we can just put all scan results (wifiPointsLocationInf) in wifiPointsLocationInfList
                wifiPointsLocationInfList.putAll(wifiPointsLocationInf);
            } else {
                Double[] newKeys = (Double[]) wifiPointsLocationInf.keySet().toArray(new Double[wifiPointsLocationInf.size()]);
                Point[] newValues = (Point[]) wifiPointsLocationInf.values().toArray(new Point[wifiPointsLocationInf.size()]);
                Double[] keys = (Double[]) wifiPointsLocationInfList.keySet().toArray(new Double[wifiPointsLocationInfList.size()]);
                Point[] values = (Point[]) wifiPointsLocationInfList.values().toArray(new Point[wifiPointsLocationInfList.size()]);
                //If no, check all new scan results (newKeys, newValues) against previous data
                for (int i = 0; i < newValues.length; i++) {
                    boolean match = false;
                    for (int j = 0; j < values.length; j++) {
                        //If wifiPointsLocationInfList already contains geo coordinates with corresponding distance to
                        //one of Wi-Fi access points from new scan
                        //if (newValues[i].getLongitude() == values[j].getLongitude()) {
                        if (newValues[i].equals(values[j])) {
                            //Check distance
                            if (newKeys[i] < keys[j]) {
                                //If distance from previous scan is bigger than one from new, replace data in wifiPointsLocationInfList
                                wifiPointsLocationInfList.remove(keys[j]);
                                wifiPointsLocationInfList.put(newKeys[i], newValues[i]);
                            }
                            match = true;
                            j = values.length;
                        }
                    }
                    if (!match) //If wifiPointsLocationInfList doesn´t contain geo coordinates with corresponding distance to one of Wi-Fi access points from new scan
                        wifiPointsLocationInfList.put(newKeys[i], newValues[i]);
                }
            }

            //If four scans were run
            if (counter == 4) {
                //Calculate position
                findPosition(c);
                //Check if there was detected movent, if yes, clear wifiPointsLocationInfList
                if (moving) {
                    wifiPointsLocationInfList.clear();
                    moving = false;
                }
                //sensorUpdateTrigger = true;
                counter = 1;
                //wifiPointsLocationInfList.clear();
            }
        }
    }

    /**
     *
     * @param c
     */
    private void findPosition(Context c) {
        //Sorting wifiPointsLocationInfList by distance
        Map<Double, Point> sortedWifiPointsLocationInf = new TreeMap<Double, Point>(wifiPointsLocationInfList);

        //Checking number of access points. If there are more than three, than keep only closest three
        if (sortedWifiPointsLocationInf.size() > 3) {
            Double[] keys = (Double[]) sortedWifiPointsLocationInf.keySet().toArray(new Double[sortedWifiPointsLocationInf.size()]);
            while (sortedWifiPointsLocationInf.size() > 3) {
                sortedWifiPointsLocationInf.remove(keys[sortedWifiPointsLocationInf.size() - 1]);
            }
        }

        //If there are three available Wi-Fi access points with known coordinates, calculating position
        double[] calculatedPosition = null;
        int floor;
        if (sortedWifiPointsLocationInf.size() < 3) {
            calculatedPosition = new double[]{0, 0};
            floor = 1;
        } else {
            calculatedPosition = calculatePosition(sortedWifiPointsLocationInf);
            floor = findFloor(sortedWifiPointsLocationInf);
        }

        if(lastSentPosition == null || !lastSentPosition.equals(new Point(calculatedPosition[0], calculatedPosition[1], floor))){
            lastSentPosition = new Point(calculatedPosition[0], calculatedPosition[1], floor);

            //Send result to application
            Intent intent = new Intent();
            intent.setAction("no.hesa.positionlibrary.Output");
            intent.putExtra("position", calculatedPosition);
            intent.putExtra("floor", floor);
            c.sendBroadcast(intent);
        }
    }

    /**
     * Find out what floor are user at
     * @param sortedWifiPointsLocationInf list of distances to three closest Wi-Fi access points with corresponding geo coordinates
     * @return floor number
     */
    private int findFloor(Map<Double, Point> sortedWifiPointsLocationInf) {
        Point[] values = (Point[]) sortedWifiPointsLocationInf.values().toArray(new Point[sortedWifiPointsLocationInf.size()]);
        ArrayList<Integer> floors = new ArrayList<>(values.length);
        ArrayList<Integer> occurences = new ArrayList<>();
        //Fill floors array with floor number corresponding to Wi-Fi access point
        for (int i = 0; i < values.length; i++) {
            floors.add(values[i].getFloor());
        }
        //Count occurences of every floor
        for (int i = 0; i < 6; i++) {
            occurences.add(Collections.frequency(floors, i));
        }
        //Get number to the most often occured floor
        int result = occurences.indexOf(Collections.max(occurences));

        return result;
    }

    /**
     * Calculate the distance between two geo coordinates (two last estimated positions)
     * @param calculatedPosition geo coordinates of newly estimated position
     * @return distance in meters
     */
    /*private float calculateDistance(double[] calculatedPosition){
        float [] dist = new float[1];
        Location.distanceBetween(lastCalculatedPosition[0], lastCalculatedPosition[1], calculatedPosition[0], calculatedPosition[1], dist);
        return dist[0];
    }*/

    /**
     * Calculate the distance to one access point given RSSI in db and frequency in MHz
     * @param levelInDb RSSI (received signal strength indication) in decibels
     * @param freqInMHz Frequency in MHz
     * @return The distance to the access-point in meters
     */
    private double distanceToAccessPoint(double levelInDb, double freqInMHz) {
        double tx_PWR = 17; //transmitter output power in dB
        double gain_TX = 4; //transmit-side antenna gain in dBi
        double gain_RX = 0; //transmit-side antenna gain in dBi
        double pl_1meter = 23; //reference path loss
        //double s = 4; //standard deviation of shadow fading
        //double n = 3.7; //path loss exponent

        double a = 3; //distance from phone to ceiling (rouge estimation)

        double FSPL = tx_PWR + gain_TX + gain_RX - levelInDb - pl_1meter; //free space path loss
        double exp = (FSPL + 27.55 - (20 * Math.log10(freqInMHz))) / 21.66;
        //double exp = (27.55 - (20 * Math.log10(freqInMHz)) + Math.abs(levelInDb)) / 20.0;
        //double exp = (tx_PWR - levelInDb + gain_TX - pl_1meter + s)/(10 * n);
        double b = Math.pow(10.0, exp);

        if(b < 3)
            a = 1.7;

        double distance = Math.sqrt(b * b - a * a);
        return distance;
    }

    private static double radiansFromDeg(double degrees) {
        return degrees * (Math.PI / 180);
    }

    private static double degFromRadians(double radians) {
        return radians * (180 / Math.PI);
    }

    public static double[] calculateCoordinates(double[] from, double distanceKm, double bearingDegrees) {
        double distanceRadians = (double) distanceKm / 6371;
        double bearingRadians = radiansFromDeg(bearingDegrees);
        double fromLatRadians = radiansFromDeg(from[0]);
        double fromLonRadians = radiansFromDeg(from[1]);
        double toLatRadians = Math.asin(Math.sin(fromLatRadians) * Math.cos(distanceRadians) + Math.cos(fromLatRadians) * Math.sin(distanceRadians) * Math.cos(bearingRadians));
        double toLonRadians = fromLonRadians + Math.atan2(Math.sin(bearingRadians) * Math.sin(distanceRadians) * Math.cos(fromLatRadians), Math.cos(distanceRadians) - Math.sin(fromLatRadians) * Math.sin(toLatRadians));

        toLonRadians = ((toLonRadians + 3 * Math.PI) % (2 * Math.PI)) - Math.PI;
        return new double[]{degFromRadians(toLatRadians), degFromRadians(toLonRadians)};
    }

    /**
     * Calculate from geo coordinates too cartesian coordinates
     */
    public static double[] convertFromGeo(double[] coordinates) {
        double x_cartesian = radius * Math.cos(coordinates[0] * Math.PI / 180) * Math.cos(coordinates[1] * Math.PI / 180);
        double y_cartesian = radius * Math.cos(coordinates[0] * Math.PI / 180) * Math.sin(coordinates[1] * Math.PI / 180);
        double z_cartesian = radius * Math.sin(coordinates[0] * Math.PI / 180);
        return new double[]{x_cartesian, y_cartesian, z_cartesian};
    }

    /**
     * Calculate from cartesian coordinates to geo coordinates
     */
    public static double[] convertToGeo(double[] coordinates) {
        double x_deg = (180 / Math.PI) * Math.asin(coordinates[2] / radius);
        double y_deg = (180 / Math.PI) * Math.atan2(coordinates[1], coordinates[0]);
        return new double[]{x_deg, y_deg};
    }

    /**
     * Match distances to available Wi-Fi access points with corresponding geo coordinates against MAC address
     * @param wifiPointsScanInf list of MAC addresses to available Wi-Fi access points with corresponding distances
     * @return list of distances to available Wi-Fi access points with corresponding geo coordinates
     */
    public HashMap<Double, Point> matchMACtoGeo(Map<String, Double> wifiPointsScanInf) {
        HashMap<Double, Point> wifiPointsLocationInf = new HashMap<Double, Point>(wifiPointsScanInf.size());
        Double[] keys = (Double[]) wifiPointsScanInf.values().toArray(new Double[wifiPointsScanInf.size()]);
        String[] values = (String[]) wifiPointsScanInf.keySet().toArray(new String[wifiPointsScanInf.size()]);

        //List of MAC addresses to known Wi-Fi access point with corresponding geo coordinates
        //TreeMap<String, Point> wifiPointsMacGeo = new TreeMap<String, Point>();
        //frequency: 5.0
        /*wifiPointsMacGeo.put("ec:bd:1d:88:8d:bf", new Point(68.43611583610615, 17.43369428932667, 1));
        wifiPointsMacGeo.put("ec:bd:1d:6b:8d:4f", new Point(68.43606297172468, 17.433767840266228, 1));
        wifiPointsMacGeo.put("ec:bd:1d:6b:8e:7f", new Point(68.43617239716085, 17.433753423392773, 1));
        wifiPointsMacGeo.put("ec:bd:1d:88:85:7f", new Point(68.43623622842107, 17.43380941450596, 1));
        wifiPointsMacGeo.put("34:ab:4e:fc:5f:4f", new Point(68.4362602575168, 17.43396732956171, 1));
        wifiPointsMacGeo.put("5c:83:8f:34:f9:5f", new Point(68.43618089979086, 17.434068247675896, 1));*/

        //Checking MAC addresses of available Wi-Fi access points against list of MAC addresses to known Wi-Fi access points
        for (int i = 0; i < wifiPointsScanInf.size(); i++) {
            //If there is a match, putting distance with corresponding geo coordinates in the wifiPointsLocationInf
            if (wifiPointsMacGeo.containsKey(values[i]))
                wifiPointsLocationInf.put(keys[i], wifiPointsMacGeo.get(values[i]));
        }
        return wifiPointsLocationInf;
    }

    /**
     * Calculate current geo coordinates
     * @param wifiPointsLocationInf list of distances to three closest Wi-Fi access points with corresponding geo coordinates
     * @return current geo coordinates
     */
    public double[] calculatePosition(Map<Double, Point> wifiPointsLocationInf) {
        double[] calculatedPosition = null;
        Point[] degrees = (Point[]) wifiPointsLocationInf.values().toArray(new Point[wifiPointsLocationInf.size()]);

        double[][] cartesian = new double[wifiPointsLocationInf.size()][3];

        for (int i = 0; i < wifiPointsLocationInf.size(); i++) {
            cartesian[i] = WifiPosition.convertFromGeo(new double[]{degrees[i].getLatitude(), degrees[i].getLongitude()});
        }
        Double[] distances = (Double[]) wifiPointsLocationInf.keySet().toArray(new Double[wifiPointsLocationInf.size()]);
        TrilaterationFunction trilaterationFunction = new TrilaterationFunction(cartesian, distances);
        LinearLeastSquaresSolver lSolver = new LinearLeastSquaresSolver(trilaterationFunction);
        NonLinearLeastSquaresSolver nlSolver = new NonLinearLeastSquaresSolver(trilaterationFunction, new LevenbergMarquardtOptimizer());
        RealVector x = lSolver.solve();
        LeastSquaresOptimizer.Optimum optimum = nlSolver.solve();

        calculatedPosition = optimum.getPoint().toArray();
        calculatedPosition = WifiPosition.convertToGeo(calculatedPosition);

        return calculatedPosition;
    }

    @Override
    public void onCompletedAction(JSONObject jsonObject, String actionString) {
        switch (actionString) {
            //If we get data about wi-fi access points
            case Api.ALL_ACCESS_POINTS:
                try {
                    if (jsonObject != null) {
                        JSONArray jsonArrayWiFi = jsonObject.getJSONArray("accesspoints");
                        for (int i = 0; i < jsonArrayWiFi.length(); i++) {
                            if (jsonArrayWiFi.get(i) != null) {
                                JSONObject jObjectWiFi = jsonArrayWiFi.getJSONObject(i);
                                JSONObject coordinate = jObjectWiFi.getJSONObject("coordinate");
                                //Fill list with information on wi-fi access points with data we get from API
                                wifiPointsMacGeo.put(jObjectWiFi.getString("macAddress"), new Point(coordinate.getDouble("lat"), coordinate.getDouble("lng"), coordinate.getInt("alt")));
                            }
                        }
                    }
                } catch (Exception e) {
                    e.printStackTrace();
                }
                break;
            // TODO: remove the following case
            case Api.ALL_PATH_POINTS:
                try {
                    if(jsonObject != null){
                        model = new ArrayList<Edge>();
                        JSONArray jsonArrayPath = jsonObject.getJSONArray("pathPoints");
                        //Traverse first level
                        for(int i = 0; i < jsonArrayPath.length(); i++){
                            //Save firstPoint in array
                            JSONObject firstPoint = jsonArrayPath.getJSONObject(i).getJSONObject("coordinate");
                            Vertex<Point> firstVertex = new Vertex<>(new Point(firstPoint.getDouble("lat"), firstPoint.getDouble("lng"), firstPoint.getInt("alt")));
                            allPathPoints.add(new Point(firstPoint.getDouble("lat"), firstPoint.getDouble("lng"), firstPoint.getInt("alt")));

                            JSONArray jsonArrayNeighbours = jsonArrayPath.getJSONObject(i).getJSONArray("neighbours");
                            //Traverse second level
                            for(int j = 0; j < jsonArrayNeighbours.length(); j++){
                                JSONObject secondPoint = jsonArrayNeighbours.getJSONObject(j).getJSONObject("coordinate");
                                Vertex<Point> secondVertex = new Vertex<>(new Point(secondPoint.getDouble("lat"), secondPoint.getDouble("lng"), secondPoint.getInt("alt")));
                                model.add(new Edge(firstVertex, secondVertex, jsonArrayNeighbours.getJSONObject(j).getInt("distance")));
                            }
                        }
                    }
                } catch (Exception e) {
                    e.printStackTrace();
                }
                break;
        }
    }

    @Override
    public void onAuthorizationFailed() {

    }

    private void buildPathPointModel(String pathPointJson) {
        try {
            JSONObject jsonObject = new JSONObject(pathPointJson);

            if(jsonObject != null){
                model = new ArrayList<Edge>();
                JSONArray jsonArrayPath = jsonObject.getJSONArray("pathPoints");
                //Traverse first level
                for(int i = 0; i < jsonArrayPath.length(); i++){
                    //Save firstPoint in array
                    JSONObject firstPoint = jsonArrayPath.getJSONObject(i).getJSONObject("coordinate");
                    Vertex<Point> firstVertex = new Vertex<>(new Point(firstPoint.getDouble("lat"), firstPoint.getDouble("lng"), firstPoint.getInt("alt")));
                    allPathPoints.add(new Point(firstPoint.getDouble("lat"), firstPoint.getDouble("lng"), firstPoint.getInt("alt")));

                    JSONArray jsonArrayNeighbours = jsonArrayPath.getJSONObject(i).getJSONArray("neighbours");
                    //Traverse second level
                    for(int j = 0; j < jsonArrayNeighbours.length(); j++){
                        JSONObject secondPoint = jsonArrayNeighbours.getJSONObject(j).getJSONObject("coordinate");
                        Vertex<Point> secondVertex = new Vertex<>(new Point(secondPoint.getDouble("lat"), secondPoint.getDouble("lng"), secondPoint.getInt("alt")));
                        model.add(new Edge(firstVertex, secondVertex, jsonArrayNeighbours.getJSONObject(j).getInt("distance")));
                    }
                }
            }
        } catch (Exception e) {
            e.printStackTrace();
        }
    }

    /**
     * Generate list with "path points" to plot the route on the map
     * @param destination geo coordinates of destination point
     * @return list with "path points" from start to destination
     * @throws PathNotFoundException
     */
    public List<Point> plotRoute(Point destination, String pathPointsJson) throws PathNotFoundException {
        buildPathPointModel(pathPointsJson);
        //Point destination = new Point(68.43609254621903, 17.434575855731964, 1);
        //Point lastSentPosition = new Point(68.43620505217169, 17.433623000979424, 1);
        //Point lastSentPosition = new Point(68.43614836797185, 17.433611266314983, 1)
        List<Point> result;

        if(lastSentPosition == null || lastSentPosition.equals(new Point(0, 0, 1))){
            result = null;
        }
        else{
            result = generateRoute(lastSentPosition, destination);
        }

        return result;
    }

    /**
     * Generate list with "path points" to plot the route on the map
     * @param position geo coordinates of start point
     * @param destination geo coordinates of destination point
     * @return list with "path points" from start to destination
     * @throws PathNotFoundException
     */
    public List<Point> plotRoute(Point position, Point destination, String pathPointsJson) throws PathNotFoundException {
        buildPathPointModel(pathPointsJson);
        List<Point> result = generateRoute(position, destination);

        return result;
    }

    public List<Point> generateRoute(Point position, Point destination) throws PathNotFoundException {
        HashMap<Point, Double> closestPathPoint = findClosestPathPoint(position);
        Point[] point = (Point[]) closestPathPoint.keySet().toArray(new Point[closestPathPoint.size()]);
        Double[] distance = (Double[]) closestPathPoint.values().toArray(new Double[closestPathPoint.size()]);
        //Add new node
        model.add(new Edge(new Vertex(position), new Vertex(point[0]), distance[0].intValue()));

        graph = new Graph(model);
        da = new DijkstraAlgorithm(graph);
        da.execute(new Vertex<>(position));
        LinkedList<Vertex> path = da.getPath(new Vertex<>(destination));

//        List<Point> result = getPointsOnCurrentFloor(path, position.getFloor());
        List<Point> result = new LinkedList<>();
        for(int i = 0; i < path.size(); i++){
            if (path.get(i).getPayload() instanceof Point){
                Point pathPoint = (Point) path.get(i).getPayload();
                result.add(pathPoint);
            }
        }

        return result;
    }

    /**
     * Find closest path point to users current position
     * @param position current position
     * @return Closest path point and distance to it
     */
    public HashMap<Point, Double> findClosestPathPoint(Point position){
        List<Float> distances = new ArrayList<>();
        HashMap<Point, Double> result = new HashMap<Point, Double>();

        for(int i = 0; i < allPathPoints.size(); i++){
            if(allPathPoints.get(i).getFloor() == position.getFloor()){
                float [] dist = new float[1];
                Location.distanceBetween(position.getLatitude(), position.getLongitude(), allPathPoints.get(i).getLatitude(), allPathPoints.get(i).getLongitude(), dist);
                distances.add(dist[0]);
            }
        }

        Float minDist = Collections.min(distances);
        result.put(allPathPoints.get(distances.indexOf(minDist)), Double.valueOf(minDist));
        return result;
    }

    /**
     * Select all path points on current floor
     * @param path list with all path points from start to destination
     * @param floor current floor (from current position)
     * @return list with all path points on current floor
     */
    public List<Point> getPointsOnCurrentFloor(LinkedList<Vertex> path, int floor){
        List<Point> result = new LinkedList<>();
        for(int i = 0; i < path.size(); i++){
            if (path.get(i).getPayload() instanceof Point){
                Point pathPoint = (Point) path.get(i).getPayload();
                if(pathPoint.getFloor() == floor)
                    result.add(pathPoint);
            }
        }

        return result;
    }
}
