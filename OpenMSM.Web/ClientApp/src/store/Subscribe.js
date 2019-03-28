import { SET_LAST_API_HEADER, ADD_HEADER_MESSAGE } from './HeaderLogging'
export const OPEN_SESSION_REQUEST = 'OPEN_SESSION_REQUEST'
export const OPEN_SESSION_RESPONSE = 'OPEN_SESSION_RESPONSE'
export const READ_PUBLICATION_REQUEST = 'READ_PUBLICATION_REQUEST'
export const READ_PUBLICATION_RESPONSE = 'READ_PUBLICATION_RESPONSE'
export const REMOVE_PUBLICATION_REQUEST = 'REMOVE_PUBLICATION_REQUEST'
export const REMOVE_PUBLICATION_RESPONSE = 'REMOVE_PUBLICATION_RESPONSE'
export const CLOSE_SUBSCRIPTION_SESSION_REQUEST = 'CLOSE_SUBSCRIPTION_SESSION_REQUEST'
export const CLOSE_SUBSCRIPTION_SESSION_RESPONSE = 'CLOSE_SUBSCRIPTION_SESSION_RESPONSE'
export const SET_ACCESS_TOKEN = 'SET_ACCESS_TOKEN'
export const CHANGE_TAB = 'CHANGE_TAB'

const initialState = {
    accessToken: '',
    activeTab: 'Open'
};

const buildResponse = (response) => {
    return {
        headers: response.headers,
        ok: response.ok,
        redirected: response.redirected,
        status: response.status,
        statusText: response.statusText,
        type: response.type,
        url: response.url
    };
};

const subscribeApiFunctions = {
    openSubscriptionSession: async (channelUri, session, accessToken, dispatch) => {
        const url = 'api/channels/' + encodeURIComponent(channelUri) + '/subscription-sessions';
        const options = {
            method: 'POST',
            headers: {
                "Authorization": accessToken,
                "Content-Type": "application/json"
            },
            body: JSON.stringify(session)
        };
        try {
            const response = await fetch(url, options);
            const session = await response.json();
            if (dispatch) {
                let builtResponse = buildResponse(response);
                builtResponse.body = session;
                dispatch({ type: SET_LAST_API_HEADER, lastApiCallUrl: url, lastApiCallDetails: options, lastApiResponse: builtResponse });
            }
            return { responseData: response, responseBody: session };
        }
        catch (e) {

        }
    },
    readPublication: async (sessionId, accessToken, dispatch) => {
        const url = 'api/sessions/' + encodeURIComponent(sessionId) + '/publication';
        const options = {
            method: 'GET',
            headers: {
                "Authorization": accessToken,
                "Content-Type": "application/json"
            }
        };
        try {
            const response = await fetch(url, options);
            const returnMessage = await response.json();
            if (dispatch) {
                let builtResponse = buildResponse(response);
                builtResponse.body = returnMessage;
                dispatch({ type: SET_LAST_API_HEADER, lastApiCallUrl: url, lastApiCallDetails: options, lastApiResponse: builtResponse });
            }
            return { responseData: response, responseBody: returnMessage };
        } catch (e) {
            console.log(e);
        }
    },
    removePublication: async (sessionId, accessToken, dispatch) => {
        const url = 'api/sessions/' + encodeURIComponent(sessionId) + '/publication';
        const options = {
            method: 'DELETE',
            headers: {
                "Authorization": accessToken,
                "Content-Type": "application/json"
            }
        };
        const response = await fetch(url, options);
        if (dispatch) {
            dispatch({ type: SET_LAST_API_HEADER, lastApiCallUrl: url, lastApiCallDetails: options, lastApiResponse: buildResponse(response) });
        }
        return response;
    },
    closeSession: async (sessionId, accessToken, dispatch) => {
        const url = 'api/sessions/' + encodeURIComponent(sessionId);
        const options = {
            method: 'DELETE',
            headers: {
                "Authorization": accessToken,
                "Content-Type": "application/json"
            }
        };
        const response = await fetch(url, options);
        if (dispatch) {
            dispatch({ type: SET_LAST_API_HEADER, lastApiCallUrl: url, lastApiCallDetails: options, lastApiResponse: buildResponse(response) });
        }
        return response;
    }
};

export const actionCreators = {
    setAccessToken: (event) => async (dispatch, getState) => {
        event.preventDefault();
        const data = new FormData(event.target);
        var object = {};
        data.forEach(function (value, key) {
            object[key] = value;
        });
        dispatch({ type: SET_ACCESS_TOKEN, accessToken: object['token'] });
    },
    setActiveTab: (tab) => async (dispatch, getState) => {
        dispatch({ type: CHANGE_TAB, tab: tab });
    },
    openSession: (event) => async (dispatch, getState) => {
        dispatch({ type: OPEN_SESSION_REQUEST });
        const accessToken = getState().subscribe.accessToken;
        const channelUri = event.form.channelUri;
        const response = await subscribeApiFunctions.openSubscriptionSession(channelUri, event.form.session, accessToken, dispatch);
        dispatch({ type: OPEN_SESSION_RESPONSE, response, channelUri });
        if (response.responseData.ok) {
            dispatch({
                type: ADD_HEADER_MESSAGE,
                message: "Channel '" + channelUri + "': A subscription session was opened with ID " + response.responseBody.id + ""
            });
        } else {
            dispatch({
                type: ADD_HEADER_MESSAGE,
                message: "FAILURE - Channel '" + channelUri + "': " + response.responseBody.message
            });
        }
        event.setFinished();
    },
    readPublication: (event) => async (dispatch, getState) => {
        dispatch({ type: READ_PUBLICATION_REQUEST });
        const sub = getState().subscribe;
        const accessToken = sub.accessToken;
        const response = await subscribeApiFunctions.readPublication(event.form.sessionId, accessToken, dispatch);
        dispatch({ type: READ_PUBLICATION_RESPONSE, response });
        if (response.responseData.ok) {
            dispatch({
                type: ADD_HEADER_MESSAGE,
                message: "Session '" + event.form.sessionId + "': A message was read. " + JSON.stringify(response.responseBody, undefined, 2)
            });
        } else {
            dispatch({
                type: ADD_HEADER_MESSAGE,
                message: "FAILURE - Session '" + event.form.sessionId + "': " + response.responseBody.message
            });
        }
        event.setFinished();
    },
    removePublication: (event) => async (dispatch, getState) => {
        dispatch({ type: REMOVE_PUBLICATION_REQUEST });
        const accessToken = getState().subscribe.accessToken;
        const message = await subscribeApiFunctions.removePublication(event.form.sessionId, accessToken, dispatch);
        dispatch({ type: REMOVE_PUBLICATION_RESPONSE, message });
        dispatch({
            type: ADD_HEADER_MESSAGE,
            message: "Session '" + event.form.sessionId + "': A publication may or may not have been removed."
        });
        event.setFinished();
    },
    closeSession: (event) => async (dispatch, getState) => {
        dispatch({ type: CLOSE_SUBSCRIPTION_SESSION_REQUEST });
        const accessToken = getState().subscribe.accessToken;
        const message = await subscribeApiFunctions.closeSession(event.form.sessionId, accessToken, dispatch);
        dispatch({ type: CLOSE_SUBSCRIPTION_SESSION_RESPONSE, message });
        dispatch({
            type: ADD_HEADER_MESSAGE,
            message: "Session '" + event.form.sessionId + "': Session was closed."
        });
        event.setFinished();
    }
};

// reducers update state
// reducers cannot contain asynchronous code
// do not modify the original state, but make sure you copy the state
export const reducer = (state, action) => {
    state = state || initialState;

    if (action.type === SET_ACCESS_TOKEN) {
        return {
            ...state,
            accessToken: action.accessToken
        };
    }

    if (action.type === CHANGE_TAB) {
        return {
            ...state,
            activeTab: action.tab
        };
    }

    return state;
};
