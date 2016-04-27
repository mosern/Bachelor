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
        //model.add(new Edge(new Vertex<>("1"), new Vertex<>("2"), 1));
        //model.add(new Edge(new Vertex<>("2"), new Vertex<>("3"), 1));
        //model.add(new Edge(new Vertex<>("1"), new Vertex<>("3"), 1));
        model.add(new Edge(new Vertex<>(new Point(68.4362574233171, 17.433836236596107, 1)), new Vertex<>(new Point(68.43619802826765, 17.434018291532993, 1)), 12));
        model.add(new Edge(new Vertex<>(new Point(68.436144794399799, 17.43381779640913, 1)), new Vertex<>(new Point(68.43619802826765, 17.434018291532993, 1)), 10));
        model.add(new Edge(new Vertex<>(new Point(68.43619802826765, 17.434018291532993, 1)), new Vertex<>(new Point(68.4361741223324, 17.43412122130394, 1)), 6));
        model.add(new Edge(new Vertex<>(new Point(68.4361741223324, 17.43412122130394, 1)), new Vertex<>(new Point(68.4361634016213, 17.434165813028812, 1)), 3));
        model.add(new Edge(new Vertex<>(new Point(68.4361741223324, 17.43412122130394, 1)), new Vertex<>(new Point(68.43614208341056, 17.434064894914627, 1)), 4));

        model.add(new Edge(new Vertex<>(new Point(68.43619802826765, 17.434018291532993, 1)), new Vertex<>(new Point(68.4362574233171, 17.433836236596107, 1)), 12));
        model.add(new Edge(new Vertex<>(new Point(68.43619802826765, 17.434018291532993, 1)), new Vertex<>(new Point(68.436144794399799, 17.43381779640913, 1)), 10));
        model.add(new Edge(new Vertex<>(new Point(68.4361741223324, 17.43412122130394, 1)), new Vertex<>(new Point(68.43619802826765, 17.434018291532993, 1)), 6));
        model.add(new Edge(new Vertex<>(new Point(68.4361634016213, 17.434165813028812, 1)), new Vertex<>(new Point(68.4361741223324, 17.43412122130394, 1)), 3));
        model.add(new Edge(new Vertex<>(new Point(68.43614208341056, 17.434064894914627, 1)), new Vertex<>(new Point(68.4361741223324, 17.43412122130394, 1)), 4));

        graph = new Graph(model);
        da = new DijkstraAlgorithm(graph);
        //da.execute(new Vertex<>("1"));
        da.execute(new Vertex<>(new Point(68.43614208341056, 17.434064894914627, 1)));

        LinkedList<Vertex> path = da.getPath(new Vertex<>(new Point(68.4362574233171, 17.433836236596107, 1)));
        assertTrue(path.size() == 4);
    }

    @Test
    public void testAllObjectsCreated() throws Exception {
        assertNotNull(da);
        assertNotNull(graph);
        assertNotNull(model);
    }

    @Test
    public void testGetPath_validInput() throws Exception {
        //LinkedList<Vertex> path = da.getPath(new Vertex<>("3"));
        //assertTrue(path.size() == 2);
        //assertTrue("1".equals(String.valueOf(path.get(0))));
        //assertTrue("3".equals(String.valueOf(path.get(1))));
    }
}