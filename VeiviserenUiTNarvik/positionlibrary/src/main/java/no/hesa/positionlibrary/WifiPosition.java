package no.hesa.positionlibrary;

import android.content.BroadcastReceiver;
import android.content.Context;
import android.content.Intent;
import android.content.IntentFilter;
import android.net.wifi.ScanResult;
import android.net.wifi.WifiManager;

import java.util.List;

/**
 * A class that takes care of positioning using wifi access-points
 */

public class WifiPosition {
    private List<ScanResult> scanResults = null;
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
    private double[] distances = null; //distances to nearest Wi-Fi access points
    private double[] helpArray = null; //helper array (used for sorting)


    public WifiPosition() {
        //Add logic here, initialize listeners, method calls and so on
    }

    public void registerBroadcast(Context c) {
        c.registerReceiver(wifiReceiver, new IntentFilter(WifiManager.SCAN_RESULTS_AVAILABLE_ACTION));
    }
    public void unRegisterBroadcast(Context c) {
        c.unregisterReceiver(wifiReceiver);
    }

    public void calculateDistances(Context c) {
        if (scanResults != null) {
            distances = new double[scanResults.size()];
            for (int i = 0; i < scanResults.size(); i++) {
                distances[i] = distanceToAccessPoint(scanResults.get(i).level, scanResults.get(i).frequency);
            }

            //Sorting array with distances to Wi-Fi access points
            helpArray = new double[distances.length];
            mergeSort(0, distances.length - 1);

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
     * Sort distances to Wi-Fi access point recursively in ascending order
     * @param low
     * @param high
     */
    private void mergeSort(int low, int high){
        if(low < high){
            int middle = low + (high - low) / 2;
            mergeSort(low, middle);
            mergeSort(middle + 1, high);
            merge(low, middle, high);
        }
    }

    /**
     * Sort and merge arrays from mergeSort
     * @param low
     * @param middle
     * @param high
     */
    private void merge(int low, int middle, int high){
        for(int i = low; i <= high; i++)
            helpArray[i] = distances[i];

        int i = low;
        int j = middle + 1;
        int k = low;

        while(i <= middle && j <= high){
            if (helpArray[i] <= helpArray[j]) {
                distances[k] = helpArray[i];
                i++;
            }
            else {
                distances[k] = helpArray[j];
                j++;
            }
            k++;
        }

        while (i <= middle) {
            distances[k] = helpArray[i];
            k++;
            i++;
        }
    }

}
