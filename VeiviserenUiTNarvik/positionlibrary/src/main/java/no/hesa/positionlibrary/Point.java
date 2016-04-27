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

    @Override
    public boolean equals(Object o) {
        if (this == o) return true;
        if (!(o instanceof Point)) return false;

        Point point = (Point) o;

        if (Double.compare(point.getLatitude(), getLatitude()) != 0) return false;
        if (Double.compare(point.getLongitude(), getLongitude()) != 0) return false;
        return getFloor() == point.getFloor();

    }

    @Override
    public int hashCode() {
        int result;
        long temp;
        temp = Double.doubleToLongBits(getLatitude());
        result = (int) (temp ^ (temp >>> 32));
        temp = Double.doubleToLongBits(getLongitude());
        result = 31 * result + (int) (temp ^ (temp >>> 32));
        result = 31 * result + getFloor();
        return result;
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
