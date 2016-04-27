package no.hesa.positionlibrary;

import org.junit.Before;
import org.junit.Test;
import org.junit.runner.RunWith;
import org.junit.runners.JUnit4;

import java.util.ArrayList;
import java.util.LinkedList;
import java.util.List;

import no.hesa.positionlibrary.dijkstra.DijkstraAlgorithm;
import no.hesa.positionlibrary.dijkstra.exception.PathNotFoundException;
import no.hesa.positionlibrary.dijkstra.model.Edge;
import no.hesa.positionlibrary.dijkstra.model.Graph;
import no.hesa.positionlibrary.dijkstra.model.Vertex;

import static junit.framework.TestCase.assertNotNull;
import static junit.framework.TestCase.assertTrue;

/**
 * To work on unit tests, switch the Test Artifact in the Build Variants view.
 */
@RunWith(JUnit4.class)
public class DijkstraUnitTest {
    private DijkstraAlgorithm da;
    private Graph graph;
    private List<Edge> model;

    @Before
    public void setUp() throws Exception {
        buildModel();
    }

    private void buildModel() throws PathNotFoundException {
        model = new ArrayList<Edge>();
        model.add(new Edge(new Vertex<>("1"), new Vertex<>("2"), 1));
        model.add(new Edge(new Vertex<>("2"), new Vertex<>("3"), 1));
        model.add(new Edge(new Vertex<>("1"), new Vertex<>("3"), 1));
        graph = new Graph(model);
        da = new DijkstraAlgorithm(graph);
        da.execute(new Vertex<>("1"));
    }

    @Test
    public void testAllObjectsCreated() throws Exception {
        assertNotNull(da);
        assertNotNull(graph);
        assertNotNull(model);
    }

    @Test
    public void testGetPath_validInput() throws Exception {
        LinkedList<Vertex> path = da.getPath(new Vertex<>("3"));
        assertTrue(path.size() == 2);
        assertTrue("1".equals(String.valueOf(path.get(0))));
        assertTrue("3".equals(String.valueOf(path.get(1))));
    }
}