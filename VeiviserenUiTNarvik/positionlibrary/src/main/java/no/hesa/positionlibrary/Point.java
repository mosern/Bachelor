package no.hesa.positionlibrary;

import java.util.Objects;

/**
 * Point class that is used to store geo coordinates (latitude and longitude) and floor number (altitude)
 */
public class Point {
    private double latitude; //latitude
    private double longitude; //longitude
    private int floor; //floor

    /**
     * Create Point object
     * @param latitude
     * @param longitude
     * @param floor
     */
    public Point (double latitude, double longitude, int floor){
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

    /**
     * Get latitude
     * @return latitude
     */
    public double getLatitude(){

        return latitude;
    }

    /**
     * Get longitude
     * @return longitude
     */
    public double getLongitude(){
        return longitude;
    }

    /**
     * Get floor
     * @return floor
     */
    public int getFloor(){
        return floor;
    }


}
