package no.hesa.positionlibrary;

import org.apache.commons.math3.fitting.leastsquares.LeastSquaresOptimizer;
import org.apache.commons.math3.fitting.leastsquares.LevenbergMarquardtOptimizer;
import org.apache.commons.math3.linear.RealMatrix;
import org.apache.commons.math3.linear.RealVector;
import org.apache.commons.math3.linear.SingularMatrixException;
import org.junit.Test;
import org.junit.runner.RunWith;
import org.junit.runners.JUnit4;

import static org.junit.Assert.*;

/**
 * To work on unit tests, switch the Test Artifact in the Build Variants view.
 */
@RunWith(JUnit4.class)
public class WiFiPositionUnitTest {
 /*   private double[][] degrees = new double[][]{{68.43605,17.43437},{68.43606,17.43439},{68.43618,17.43468}};
    private double[][] degrees_test = new double[][]{{68.43617,17.43358},{68.43623,17.43363},{68.43629,17.43363}};
    private double[][] cartesian = null;

    private double x_expected_degree = 68.43622; //x-coordinate for expected position
    private double y_expected_degree = 17.4338; //y-coordinate for expected position

    private double radius = 6371 * 1000; //earth raduis (meters)*/

    @Test
    public void testPosition() {
        /*double[] expectedPosition = new double[] {x_expected_degree, y_expected_degree};
        cartesian = new double[degrees.length][3];
        for (int i = 0; i < degrees.length; i++) {
            cartesian[i] = WifiPosition.convertFromGeo(degrees[i]);
        }
        double[] distances = new double[] {4.7,4.5,18.2};
        double[] distances_test = new double[]{11.11,6.52,10.52};
        TrilaterationFunction trilaterationFunction = new TrilaterationFunction(cartesian,distances);
        LinearLeastSquaresSolver lSolver = new LinearLeastSquaresSolver(trilaterationFunction);
        NonLinearLeastSquaresSolver nlSolver = new NonLinearLeastSquaresSolver(trilaterationFunction, new LevenbergMarquardtOptimizer());
        RealVector x = lSolver.solve();
        LeastSquaresOptimizer.Optimum optimum = nlSolver.solve();

        testResults(expectedPosition,0.0001,optimum,x);*/
        double[] arr = WifiPosition.calculateCoordinates(new double[]{68.43605083,17.4343663},(4.692)/1000,69.416);
        System.out.println("lat: "+arr[0]+" lon: "+arr[1]);
    }

    private void testResults(double[] expectedPosition, final double delta, LeastSquaresOptimizer.Optimum optimum, RealVector x) {

        double[] calculatedPosition = optimum.getPoint().toArray();
        //Converting calculated results from Cartesian coordinates to longitude\latitude
        calculatedPosition = WifiPosition.convertToGeo(calculatedPosition);

        int numberOfIterations = optimum.getIterations();
        int numberOfEvaluations = optimum.getEvaluations();

        StringBuilder output = new StringBuilder("expectedPosition: ");
        for (int i = 0; i < expectedPosition.length; i++) {
            output.append(expectedPosition[i]).append(" ");
        }
        output.append("\n");
        output.append("linear calculatedPosition: ");
        double[] linearCalculatedPosition = x.toArray();
        for (int i = 0; i < linearCalculatedPosition.length; i++) {
            output.append(linearCalculatedPosition[i]).append(" ");
        }
        output.append("\n");
        output.append("non-linear calculatedPosition: ");
        for (int i = 0; i < calculatedPosition.length; i++) {
            output.append(calculatedPosition[i]).append(" ");
        }
        output.append("\n");

        output.append("numberOfIterations: ").append(numberOfIterations).append("\n");
        output.append("numberOfEvaluations: ").append(numberOfEvaluations).append("\n");
        try {
            RealVector standardDeviation = optimum.getSigma(0);
            output.append("standardDeviation: ").append(standardDeviation).append("\n");
            RealMatrix covarianceMatrix = optimum.getCovariances(0);
            output.append("covarianceMatrix: ").append(covarianceMatrix).append("\n");
        } catch (SingularMatrixException e) {
            System.err.println(e.getMessage());
        }

        System.out.println(output.toString());

        // expected == calculated?
        for (int i = 0; i < calculatedPosition.length; i++) {
            assertEquals(expectedPosition[i], calculatedPosition[i], delta);
        }
    }
}