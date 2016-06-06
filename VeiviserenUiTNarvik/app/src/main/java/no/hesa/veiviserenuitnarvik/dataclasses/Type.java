package no.hesa.veiviserenuitnarvik.dataclasses;

/**
 * Type data class
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
