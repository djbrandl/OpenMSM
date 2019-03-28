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

const responseApiFunctions = {
    openResponseSession: async (channelUri, session, accessToken, dispatch) => {
        const url = 'api/channels/' + encodeURIComponent(channelUri) + '/provider-request-sessions';
        const options = {
            method: 'POST',
            headers: {
                "Authorization": accessToken,
                "Content-Type": "application/json"
            },
            body: JSON.stringify(session)
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
    readRequest: async (sessionId, accessToken, dispatch) => {
        const url = 'api/sessions/' + encodeURIComponent(sessionId) + '/request';
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
    removeRequest: async (sessionId, accessToken, dispatch) => {
        const url = 'api/sessions/' + encodeURIComponent(sessionId) + '/request';
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
    postResponse: async (sessionId, requestMessageId, message, accessToken, dispatch) => {
        const url = 'api/sessions/' + encodeURIComponent(sessionId) + '/responses?requestMessageId=' + encodeURIComponent(requestMessageId);
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
        dispatch({ type: ASYNC_REQUEST_REQUEST, data: 'Open response session' });
        const accessToken = getState().respond.accessToken;
        const response = await responseApiFunctions.openResponseSession(event.form.channelUri, event.form.session, accessToken, dispatch);
        dispatch({ type: ASYNC_REQUEST_RESPOSNE, data: response });
        if (response.responseData.ok) {
            dispatch({ type: ADD_HEADER_MESSAGE, message: "Channel '" + event.form.channelUri + "': A provider request (response) session was opened with ID " + response.responseBody.id + "" });
        } else {
            dispatch({ type: ADD_HEADER_MESSAGE, message: "FAILURE - Channel '" + event.form.channelUri + "': " + response.responseBody.message });
        }
        event.setFinished();
    },
    readRequest: (event) => async (dispatch, getState) => {
        dispatch({ type: ASYNC_REQUEST_REQUEST, data: 'Read request' });
        const accessToken = getState().respond.accessToken;
        const response = await responseApiFunctions.readRequest(event.form.sessionId, accessToken, dispatch);
        dispatch({ type: ASYNC_REQUEST_RESPOSNE, data: response });
        if (response.responseData.ok) {
            dispatch({
                type: ADD_HEADER_MESSAGE, message: "Session '" + event.form.sessionId + "': A message was read. " + JSON.stringify(response.responseBody, undefined, 2)
            });
        } else {
            dispatch({ type: ADD_HEADER_MESSAGE, message: "FAILURE - Session '" + event.form.sessionId + "': " + response.responseBody.message });
        }
        event.setFinished();
    },
    removeRequest: (event) => async (dispatch, getState) => {
        dispatch({ type: ASYNC_REQUEST_REQUEST, data: 'Remove response' });
        const accessToken = getState().respond.accessToken;
        const response = await responseApiFunctions.removeRequest(event.form.sessionId, accessToken, dispatch);
        dispatch({ type: ASYNC_REQUEST_RESPOSNE, data: response });
        if (response.responseData.ok) {
            dispatch({ type: ADD_HEADER_MESSAGE, message: "Session '" + event.form.sessionId + "': A request message was removed." });
        } else {
            dispatch({ type: ADD_HEADER_MESSAGE, message: "FAILURE - Session '" + event.form.sessionId + "': " + response.responseBody.message });
        }
        event.setFinished();
    },
    postResponse: (event) => async (dispatch, getState) => {
        dispatch({ type: ASYNC_REQUEST_REQUEST, data: 'Post response' });
        const accessToken = getState().respond.accessToken;
        const response = await responseApiFunctions.postResponse(event.form.sessionId, event.form.requestMessageId, event.form.message, accessToken, dispatch);
        dispatch({ type: ASYNC_REQUEST_RESPOSNE, data: response });
        if (response.responseData.ok) {
            dispatch({ type: ADD_HEADER_MESSAGE, message: "Session '" + event.form.sessionId + "': A response message has been posted with ID " + response.responseBody.id + "" });
        } else {
            dispatch({ type: ADD_HEADER_MESSAGE, message: "FAILURE - Session '" + event.form.sessionId + "': " + response.responseBody.message });
        }
        event.setFinished();
    },
    closeSession: (event) => async (dispatch, getState) => {
        dispatch({ type: ASYNC_REQUEST_REQUEST, data: 'Close session' });
        const accessToken = getState().respond.accessToken;
        const response = await responseApiFunctions.closeSession(event.form.sessionId, accessToken, dispatch);
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
