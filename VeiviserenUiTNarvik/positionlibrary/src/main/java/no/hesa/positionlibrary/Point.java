package no.hesa.positionlibrary;

/**
 * Created by evgeniia on 08.04.16.
 */
public class Point {
    double latitude;
    double longitude;

    Point (double latitude, double longitude){
        this.latitude = latitude;
        this.longitude = longitude;
    }

    public double getLatitude(){
        return latitude;
    }

    public double getLongitude(){
        return longitude;
    }
}
