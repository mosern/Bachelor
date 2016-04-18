package no.hesa.veiviserenuitnarvik.dataclasses;

/**
 * Created by rhymf on 16.04.2016.
 */
public class Location extends Object {
    private Coordinate coordinate;
    private Type type;
    private String name;
    private String locNr;
    private int hits;
    private int id;

    public Location()
    {

    }

    @Override
    public String toString()
    {
        return coordinate.toString() + " " + type.toString()  + " Name: " + name + " LocNr: " + locNr  + " Hits: " +hits  + " Id: " +id;
    }

    public Coordinate getCoordinate() {
        return coordinate;
    }

    public void setCoordinate(Coordinate coordinate) {
        this.coordinate = coordinate;
    }

    public Type getType() {
        return type;
    }

    public void setType(Type type) {
        this.type = type;
    }

    public String getName() {
        return name;
    }

    public void setName(String name) {
        this.name = name;
    }

    public String getLocNr() {
        return locNr;
    }

    public void setLocNr(String locNr) {
        this.locNr = locNr;
    }

    public int getHits() {
        return hits;
    }

    public void setHits(int hits) {
        this.hits = hits;
    }

    public int getId() {
        return id;
    }

    public void setId(int id) {
        this.id = id;
    }
}
