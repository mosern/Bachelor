package no.hesa.positionlibrary;

/**
 * Created by evgeniia on 08.04.16.
 */
public class Point {
    private double latitude;
    private double longitude;
    private int floor;

    Point (double latitude, double longitude, int floor){
        this.latitude = latitude;
        this.longitude = longitude;
        this.floor = floor;
    }

    public double getLatitude(){
        return latitude;
    }

    public double getLongitude(){
        return longitude;
    }

    public int getFloor(){
        return floor;
    }
}
