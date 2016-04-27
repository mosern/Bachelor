package no.hesa.veiviserenuitnarvik.dataclasses;

/**
 * Created by rhymf on 16.04.2016.
 */
public class Coordinate{

    private double lng;
    private double lat;
    private double alt;
    private int id;

    public Coordinate() {

    }

    public Coordinate(int id, double lat, double lng, double alt)
    {
        this.id = id;
        this.lat = lat;
        this.lng = lng;
        this.alt = alt;
    }

    @Override
    public String toString()
    {
        return " Lng: " + lng + " Lat: " + lat  + " Alt: " + alt  + " CoordId: " + id;
    }

    public double getLng() {
        return lng;
    }

    public void setLng(double lng) {
        this.lng = lng;
    }

    public double getLat() {
        return lat;
    }

    public void setLat(double lat) {
        this.lat = lat;
    }

    public double getAlt() {
        return alt;
    }

    public void setAlt(double alt) {
        this.alt = alt;
    }

    public int getId() {
        return id;
    }

    public void setId(int id) {
        this.id = id;
    }
}
