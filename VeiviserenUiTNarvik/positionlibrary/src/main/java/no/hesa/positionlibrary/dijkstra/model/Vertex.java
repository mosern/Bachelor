package no.hesa.positionlibrary.dijkstra.model;

/***********************************************************************************
 * Author: Rune M. Andersen, Damian Łopata, Jan Inge Husby
 * Date: 23.02.2016
 * Title: DijkstraLib
 * (Version 3.1) [Computer program]
 * Available at https://github.com/wtw-software/DijkstraLib (Accessed 29 April 2016)
 **********************************************************************************/

public class Vertex<T> {

    final T payload;

    public Vertex(T payload) {
        this.payload = payload;
    }

    public T getPayload() {
        return payload;
    }

    @Override
    public boolean equals(Object other) {
        try {
            return equals((Vertex) other);
        } catch (ClassCastException e) {
            return false;
        }
    }

    public boolean equals(Vertex other) {
        return payload.equals(other.getPayload());
    }

    @Override
    public int hashCode() {
        return payload.hashCode();
    }

    @Override
    public String toString() {
        return payload.toString();
    }

} 
