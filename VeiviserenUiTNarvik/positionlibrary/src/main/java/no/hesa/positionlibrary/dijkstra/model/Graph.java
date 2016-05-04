package no.hesa.positionlibrary.dijkstra.model;

/***********************************************************************************
 * Author: Rune M. Andersen, Damian Łopata, Jan Inge Husby
 * Date: 23.02.2016
 * Title: DijkstraLib
 * (Version 3.1) [Computer program]
 * Available at https://github.com/wtw-software/DijkstraLib (Accessed 29 April 2016)
 **********************************************************************************/

import java.util.List;

public class Graph {

    private final List<Edge> edges;

    public Graph(List<Edge> edges) {
        this.edges = edges;
    }

    public List<Edge> getEdges() {
        return edges;
    }

}
