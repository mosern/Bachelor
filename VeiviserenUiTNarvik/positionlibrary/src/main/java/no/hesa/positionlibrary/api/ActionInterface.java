package no.hesa.positionlibrary.api;

import org.json.JSONObject;

public interface ActionInterface {
    void onCompletedAction(JSONObject jsonObject, String actionString);
    void onAuthorizationFailed();
}
