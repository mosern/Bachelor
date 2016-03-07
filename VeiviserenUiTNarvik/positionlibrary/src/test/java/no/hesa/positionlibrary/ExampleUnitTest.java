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
public class ExampleUnitTest {
    double x_expected = 0;
    double y_expected = 0;
    double z_expected = 0;


    @Test
    public void addition_isCorrect() throws Exception {
        assertEquals(4, 2 + 2);
    }

    @Test
    public void TestPosition() {
        double x1 = 6371 * 1000 * Math.cos(68.43618) * Math.cos(17.43358);
        double y1 = 6371 * 1000 * Math.cos(68.43618) * Math.sin(17.43358);
        double x2 = 6371 * 1000 * Math.cos(68.43629) * Math.cos(17.43362);
        double y2 = 6371 * 1000 * Math.cos(68.43629) * Math.sin(17.43362);
        double x3 = 6371 * 1000 * Math.cos(68.43623) * Math.cos(17.43362);
        double y3 = 6371 * 1000 * Math.cos(68.43623) * Math.sin(17.43362);

        x_expected = 6371 * 1000 * Math.cos(68.43622) * Math.cos(17.43377);
        y_expected = 6371 * 1000 * Math.cos(68.43622) * Math.sin(17.43377);
        z_expected = 6371 * 1000 * Math.sin(68.43622);

        double[][] positions = new double[][]{{x1, y1},{x2, y2},{x3, y3}};
        double[] distances = new double[] {9.22,9.97,5.57};
        TrilaterationFunction trilaterationFunction = new TrilaterationFunction(positions,distances);
        LinearLeastSquaresSolver lSolver = new LinearLeastSquaresSolver(trilaterationFunction);
        NonLinearLeastSquaresSolver nlSolver = new NonLinearLeastSquaresSolver(trilaterationFunction, new LevenbergMarquardtOptimizer());
        //double[] expectedPosition = new double[] {x_expected,y_expected};
        double[] expectedPosition = new double[] {68.43622,17.43377};
        RealVector x = lSolver.solve();
        LeastSquaresOptimizer.Optimum optimum = nlSolver.solve();
        testResults(expectedPosition,0.0001,optimum,x);
    }
    private void testResults(double[] expectedPosition, final double delta, LeastSquaresOptimizer.Optimum optimum, RealVector x) {

        double[] calculatedPosition = optimum.getPoint().toArray();

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

        calculatedPosition[0] = (180/Math.PI) * Math.asin(z_expected/(6371 * 1000));
        calculatedPosition[1] = (180/Math.PI) * Math.atan2(y_expected, x_expected);

        // expected == calculated?
        for (int i = 0; i < calculatedPosition.length; i++) {
            assertEquals(expectedPosition[i], calculatedPosition[i], delta);
        }
    }
}