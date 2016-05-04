package no.hesa.positionlibrary.dijkstra.exception;

/***********************************************************************************
 * Author: Rune M. Andersen, Damian Łopata, Jan Inge Husby
 * Date: 23.02.2016
 * Title: DijkstraLib
 * (Version 3.1) [Computer program]
 * Available at https://github.com/wtw-software/DijkstraLib (Accessed 29 April 2016)
 **********************************************************************************/

public class PathNotFoundException extends Exception {

    public PathNotFoundException() {
        super("Path from source to destination vertex was not found");
    }

    public PathNotFoundException(String msg) {
        super(msg);
    }

}
