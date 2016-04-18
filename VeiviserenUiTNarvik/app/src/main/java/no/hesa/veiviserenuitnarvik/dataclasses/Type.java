package no.hesa.veiviserenuitnarvik.dataclasses;

/**
 * Created by rhymf on 16.04.2016.
 */
public class Type{

    private String name;
    private int id;

    public Type()
    {

    }

    @Override
    public String toString()
    {
        return " TypeName: " + name + " TypeId: " + id;
    }

    public String getName() {
        return name;
    }

    public void setName(String name) {
        this.name = name;
    }

    public int getId() {
        return id;
    }

    public void setId(int id) {
        this.id = id;
    }
}
