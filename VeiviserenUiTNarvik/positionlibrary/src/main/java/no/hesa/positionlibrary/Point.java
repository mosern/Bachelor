package no.hesa.positionlibrary;

import java.util.Objects;

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

    @Override
    public boolean equals(Object object){
        if(object == null)
            return false;
        if (!Point.class.isAssignableFrom(object.getClass()))
            return false;
        Point point = (Point) object;
        if(this.longitude != point.longitude)
            return false;
        if(this.latitude != point.latitude)
            return false;
        if(this.floor != point.floor)
            return false;
        return true;
    }


}
