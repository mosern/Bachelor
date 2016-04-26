package no.hesa.positionlibrary.api;

import org.json.JSONObject;

/**
 * Created by ThaSimon on 09.04.2016.
 */
public interface ActionInterface {
    void onCompletedAction(JSONObject jsonObject, String actionString);
    void onAuthorizationFailed();
}
