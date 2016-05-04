package no.hesa.positionlibrary.dijkstra.model;

/***********************************************************************************
 * Author: Rune M. Andersen, Damian Łopata, Jan Inge Husby
 * Date: 23.02.2016
 * Title: DijkstraLib
 * (Version 3.1) [Computer program]
 * Available at https://github.com/wtw-software/DijkstraLib (Accessed 29 April 2016)
 **********************************************************************************/

public class Edge {

    private final Vertex source;
    private final Vertex destination;
    private final int weight;

    public Edge(Vertex source, Vertex destination, int weight) {
        this.source = source;
        this.destination = destination;
        this.weight = weight;
    }

    public Vertex getDestination() {
        return destination;
    }

    public Vertex getSource() {
        return source;
    }

    public int getWeight() {
        return weight;
    }

    @Override
    public String toString() {
        return source + " -(" + weight + ")- " + destination;
    }

}
