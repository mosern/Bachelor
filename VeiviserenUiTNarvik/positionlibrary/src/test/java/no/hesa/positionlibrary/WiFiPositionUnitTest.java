package no.hesa.positionlibrary;

import org.apache.commons.math3.fitting.leastsquares.LeastSquaresOptimizer;
import org.apache.commons.math3.fitting.leastsquares.LevenbergMarquardtOptimizer;
import org.apache.commons.math3.linear.RealMatrix;
import org.apache.commons.math3.linear.RealVector;
import org.apache.commons.math3.linear.SingularMatrixException;
import org.junit.Test;

import static org.junit.Assert.*;

/**
 * To work on unit tests, switch the Test Artifact in the Build Variants view.
 */
public class WiFiPositionUnitTest {
    private double x1_degree = 68.43618; //x-coordinate for fist Wi-Fi access point
    private double y1_degree = 17.43358; //y-coordinate for fist Wi-Fi access point
    private double x2_degree = 68.43629; //x-coordinate for second Wi-Fi access point
    private double y2_degree = 17.43362; //y-coordinate for second Wi-Fi access point
    private double x3_degree = 68.43623; //x-coordinate for third Wi-Fi access point
    private double y3_degree = 17.43362; //y-coordinate for third Wi-Fi access point

    private double x_expected_degree = 68.43622; //x-coordinate for expected position
    private double y_expected_degree = 17.43377; //y-coordinate for expected position

    private double x_expected_cartesian = 0; //x-coordinate for expected position
    private double y_expected_cartesian = 0; //y-coordinate for expected position
    private double z_expected_cartesian = 0; //z-coordinate for expected position

    private double radius = 6371 * 1000; //earth raduis (meters)

    @Test
    public void TestPosition() {
        //Converting from longitude\latitude to Cartesian coordinates
        double x1_cartesian = radius * Math.cos(x1_degree * Math.PI/180) * Math.cos(y1_degree * Math.PI/180);
        double y1_cartesian = radius * Math.cos(x1_degree * Math.PI/180) * Math.sin(y1_degree * Math.PI / 180);
        double x2_cartesian = radius * Math.cos(x2_degree * Math.PI/180) * Math.cos(y2_degree * Math.PI / 180);
        double y2_cartesian = radius * Math.cos(x2_degree * Math.PI/180) * Math.sin(y2_degree * Math.PI / 180);
        double x3_cartesian = radius * Math.cos(x3_degree * Math.PI/180) * Math.cos(y3_degree * Math.PI / 180);
        double y3_cartesian = radius * Math.cos(x3_degree * Math.PI/180) * Math.sin(y3_degree * Math.PI / 180);

        //Calculating Cartesian coordinates for expected position
        x_expected_cartesian = radius * Math.cos(x_expected_degree * Math.PI/180) * Math.cos(y_expected_degree * Math.PI/180);
        y_expected_cartesian = radius * Math.cos(x_expected_degree * Math.PI/180) * Math.sin(y_expected_degree * Math.PI/180);
        z_expected_cartesian = radius * Math.sin(x_expected_degree * Math.PI/180);

        double[][] positions = new double[][]{{x1_cartesian, y1_cartesian},{x2_cartesian, y2_cartesian},{x3_cartesian, y3_cartesian}};
        double[] distances = new double[] {9.22,9.97,5.57};
        TrilaterationFunction trilaterationFunction = new TrilaterationFunction(positions,distances);
        LinearLeastSquaresSolver lSolver = new LinearLeastSquaresSolver(trilaterationFunction);
        NonLinearLeastSquaresSolver nlSolver = new NonLinearLeastSquaresSolver(trilaterationFunction, new LevenbergMarquardtOptimizer());
        double[] expectedPosition = new double[] {x_expected_degree, y_expected_degree};
        RealVector x = lSolver.solve();
        LeastSquaresOptimizer.Optimum optimum = nlSolver.solve();

        testResults(expectedPosition,0.0001,optimum,x);
    }

    private void testResults(double[] expectedPosition, final double delta, LeastSquaresOptimizer.Optimum optimum, RealVector x) {

        double[] calculatedPosition = optimum.getPoint().toArray();
        //Converting calculated results from Cartesian coordinates to longitude\latitude
        calculatedPosition[0] = (180/Math.PI) * Math.asin(z_expected_cartesian/(radius));
        calculatedPosition[1] = (180/Math.PI) * Math.atan2(y_expected_cartesian, x_expected_cartesian);

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