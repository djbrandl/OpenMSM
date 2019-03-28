import { SET_LAST_API_HEADER, ADD_HEADER_MESSAGE } from './HeaderLogging'
export const ASYNC_REQUEST_REQUEST = 'ASYNC_REQUEST_REQUEST'
export const ASYNC_REQUEST_RESPOSNE = 'ASYNC_REQUEST_RESPOSNE'
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

const requestApiFunctions = {
    openRequestSession: async (channelUri, listenerURL, accessToken, dispatch) => {
        const url = 'api/channels/' + encodeURIComponent(channelUri) + '/consumer-request-sessions';
        const options = {
            method: 'POST',
            headers: {
                "Authorization": accessToken,
                "Content-Type": "application/json"
            },
            body: JSON.stringify({ listenerURL })
        };
        const response = await fetch(url, options);
        const returnMessage = await response.json();
        if (dispatch) {
            let builtResponse = buildResponse(response);
            builtResponse.body = returnMessage;
            dispatch({ type: SET_LAST_API_HEADER, lastApiCallUrl: url, lastApiCallDetails: options, lastApiResponse: builtResponse });
        }
        return { responseData: response, responseBody: returnMessage };
    },
    postRequest: async (sessionId, message, accessToken, dispatch) => {
        const url = 'api/sessions/' + encodeURIComponent(sessionId) + '/requests';
        const options = {
            method: 'POST',
            headers: {
                "Authorization": accessToken,
                "Content-Type": "application/json"
            },
            body: JSON.stringify(message)
        };
        const response = await fetch(url, options);
        const returnMessage = await response.json();
        if (dispatch) {
            let builtResponse = buildResponse(response);
            builtResponse.body = returnMessage;
            dispatch({ type: SET_LAST_API_HEADER, lastApiCallUrl: url, lastApiCallDetails: options, lastApiResponse: builtResponse });
        }
        return { responseData: response, responseBody: returnMessage };
    },
    readResponse: async (sessionId, requestMessageId, accessToken, dispatch) => {
        const url = 'api/sessions/' + encodeURIComponent(sessionId) + '/responses?requestMessageId=' + encodeURIComponent(requestMessageId);
        const options = {
            method: 'GET',
            headers: {
                "Authorization": accessToken,
                "Content-Type": "application/json"
            }
        };
        const response = await fetch(url, options);
        const returnMessage = await response.json();
        if (dispatch) {
            let builtResponse = buildResponse(response);
            builtResponse.body = returnMessage;
            dispatch({ type: SET_LAST_API_HEADER, lastApiCallUrl: url, lastApiCallDetails: options, lastApiResponse: builtResponse });
        }
        return { responseData: response, responseBody: returnMessage };
    },
    removeResponse: async (sessionId, requestMessageId, accessToken, dispatch) => {
        const url = 'api/sessions/' + encodeURIComponent(sessionId) + '/responses/' + encodeURIComponent(requestMessageId);
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
        return { responseData: response, responseBody: {} };
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
        return { responseData: response, responseBody: {} };
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
        dispatch({ type: ASYNC_REQUEST_REQUEST, data: 'Open request session' });
        const accessToken = getState().request.accessToken;
        const response = await requestApiFunctions.openRequestSession(event.form.channelUri, event.form.listenerURL, accessToken, dispatch);
        dispatch({ type: ASYNC_REQUEST_RESPOSNE, data: response });
        if (response.responseData.ok) {
            dispatch({ type: ADD_HEADER_MESSAGE, message: "Channel '" + event.form.channelUri + "': A consumer request session was opened with ID " + response.responseBody.id + "" });
        } else {
            dispatch({ type: ADD_HEADER_MESSAGE, message: "FAILURE - Channel '" + event.form.channelUri + "': " + response.responseBody.message });
        }
        event.setFinished();
    },
    postRequest: (event) => async (dispatch, getState) => {
        dispatch({ type: ASYNC_REQUEST_REQUEST, data: 'Post request' });
        const accessToken = getState().request.accessToken;
        const response = await requestApiFunctions.postRequest(event.form.sessionId, event.form.message, accessToken, dispatch);
        dispatch({ type: ASYNC_REQUEST_RESPOSNE, data: response });
        if (response.responseData.ok) {
            dispatch({ type: ADD_HEADER_MESSAGE, message: "Session '" + event.form.sessionId + "': A request message has been posted with ID " + response.responseBody.id + "" });
        } else {
            dispatch({ type: ADD_HEADER_MESSAGE, message: "FAILURE - Session '" + event.form.sessionId + "': " + response.responseBody.message });
        }
        event.setFinished();
    },
    readResponse: (event) => async (dispatch, getState) => {
        dispatch({ type: ASYNC_REQUEST_REQUEST, data: 'Read response' });
        const accessToken = getState().request.accessToken;
        const response = await requestApiFunctions.readResponse(event.form.sessionId, event.form.requestMessageId, accessToken, dispatch);
        dispatch({ type: ASYNC_REQUEST_RESPOSNE, data: response });
        if (response.responseData.ok) {
            dispatch({ type: ADD_HEADER_MESSAGE, message: "Session '" + event.form.sessionId + "': A message was read. " + JSON.stringify(response.responseBody, undefined, 2)
            });
        } else {
            dispatch({ type: ADD_HEADER_MESSAGE, message: "FAILURE - Session '" + event.form.sessionId + "': " + response.responseBody.message });
        }
        event.setFinished();
    },
    removeResponse: (event) => async (dispatch, getState) => {
        dispatch({ type: ASYNC_REQUEST_REQUEST, data: 'Remove response' });
        const accessToken = getState().request.accessToken;
        const response = await requestApiFunctions.removeResponse(event.form.sessionId, event.form.requestMessageId, accessToken, dispatch);
        dispatch({ type: ASYNC_REQUEST_RESPOSNE, data: response });
        if (response.responseData.ok) {
            dispatch({ type: ADD_HEADER_MESSAGE, message: "Session '" + event.form.sessionId + "': Message " + event.form.requestMessageId + " was removed." });
        } else {
            dispatch({ type: ADD_HEADER_MESSAGE, message: "FAILURE - Session '" + event.form.sessionId + "': " + response.responseBody.message });
        }
        event.setFinished();
    },
    closeSession: (event) => async (dispatch, getState) => {
        dispatch({ type: ASYNC_REQUEST_REQUEST, data: 'Close session' });
        const accessToken = getState().request.accessToken;
        const response = await requestApiFunctions.closeSession(event.form.sessionId, accessToken, dispatch);
        dispatch({ type: ASYNC_REQUEST_RESPOSNE, data: response });
        if (response.responseData.ok) {
            dispatch({ type: ADD_HEADER_MESSAGE, message: "Session '" + event.form.sessionId + "': Session was closed." });
        } else {
            dispatch({ type: ADD_HEADER_MESSAGE, message: "FAILURE - Session '" + event.form.sessionId + "': " + response.responseBody.message });
        }
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
