package no.hesa.veiviserenuitnarvik.dataclasses;

/**
 * Person data class
 */
public class Person extends Object {

    private Location location;
    private String name;
    private String tlfOffice;
    private String tlfMobile;
    private String email;
    private int locationId;
    private int id;

    public Person()
    {

    }

    @Override
    public String toString()
    {
        return "Name: " + name + " TlfOffice: " + tlfOffice + " TlfMobile: " + tlfMobile + " Email: " + email + " LocationId: " + locationId + " PersonId: " + id;
    }

    public Location getLocation() {
        return location;
    }

    public void setLocation(Location location) {
        this.location = location;
    }

    public String getName() {
        return name;
    }

    public void setName(String name) {
        this.name = name;
    }

    public String getTlfOffice() {
        return tlfOffice;
    }

    public void setTlfOffice(String tlfOffice) {
        this.tlfOffice = tlfOffice;
    }

    public String getTlfMobile() {
        return tlfMobile;
    }

    public void setTlfMobile(String tlfMobile) {
        this.tlfMobile = tlfMobile;
    }

    public String getEmail() {
        return email;
    }

    public void setEmail(String email) {
        this.email = email;
    }

    public int getLocationId() {
        return locationId;
    }

    public void setLocationId(int locationId) {
        this.locationId = locationId;
    }

    public int getId() {
        return id;
    }

    public void setId(int id) {
        this.id = id;
    }
}
