import { SET_LAST_API_HEADER, ADD_HEADER_MESSAGE } from './HeaderLogging'
export const OPEN_SESSION_REQUEST = 'OPEN_SESSION_REQUEST'
export const OPEN_SESSION_RESPONSE = 'OPEN_SESSION_RESPONSE'
export const POST_PUBLICATION_REQUEST = 'POST_PUBLICATION_REQUEST'
export const POST_PUBLICATION_RESPONSE = 'POST_PUBLICATION_RESPONSE'
export const EXPIRE_PUBLICATION_REQUEST = 'EXPIRE_PUBLICATION_REQUEST'
export const EXPIRE_PUBLICATION_RESPONSE = 'EXPIRE_PUBLICATION_RESPONSE'
export const SET_ACCESS_TOKEN = 'SET_ACCESS_TOKEN'
export const CLOSE_PUBLICATION_SESSION_REQUEST = 'CLOSE_PUBLICATION_SESSION_REQUEST'
export const CLOSE_PUBLICATIONSESSION_RESPONSE = 'CLOSE_PUBLICATIONSESSION_RESPONSE'
export const CHANGE_TAB = 'CHANGE_TAB'

const initialState = {
    accessToken: '',
    activeTab: 'Open'
    //headerLogging: {
    //    activeHeaderTab: 'LastCall',
    //    lastApiCall: { url: '', details: {}, response: {} },
    //    messages: []
    //}
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

const publishApiFunctions = {
    openPublicationSession: async (channelUri, accessToken, dispatch) => {
        const url = 'api/channels/' + encodeURIComponent(channelUri) + '/publication-sessions';
        const options = {
            method: 'POST',
            headers: {
                "Authorization": accessToken,
                "Content-Type": "application/json"
            }
        };
        try {
            const response = await fetch(url, options);
            const session = await response.json();
            if (dispatch) {
                let builtResponse = buildResponse(response);
                builtResponse.body = session;
                dispatch({ type: SET_LAST_API_HEADER, lastApiCallUrl: url, lastApiCallDetails: options, lastApiResponse: builtResponse });
            }
            return session;
        }
        catch (e) {

        }
    },
    postPublication: async (sessionId, message, accessToken, dispatch) => {
        const url = 'api/sessions/' + encodeURIComponent(sessionId) + '/publications';
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
    expirePublication: async (sessionId, messageId, accessToken, dispatch) => {
        const url = 'api/sessions/' + encodeURIComponent(sessionId) + '/publications/' + encodeURIComponent(messageId);
        const options = {
            method: 'POST',
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
        const accessToken = getState().publish.accessToken;
        const channelUri = event.form.channelUri;
        const publicationSession = await publishApiFunctions.openPublicationSession(channelUri, accessToken, dispatch);
        dispatch({ type: OPEN_SESSION_RESPONSE, publicationSession, channelUri });
        dispatch({
            type: ADD_HEADER_MESSAGE,
            message: "Channel '" + channelUri + "': A publication session was opened with ID '" + publicationSession.id + "'"
        });
        event.setFinished();
    },
    postPublication: (event) => async (dispatch, getState) => {
        dispatch({ type: POST_PUBLICATION_REQUEST });
        const accessToken = getState().publish.accessToken;
        const response = await publishApiFunctions.postPublication(event.form.sessionId, event.form.message, accessToken, dispatch);
        dispatch({ type: POST_PUBLICATION_RESPONSE, response });
        if (response.responseData.ok) {
            dispatch({
                type: ADD_HEADER_MESSAGE,
                message: "Session '" + event.form.sessionId + "': A publication message was created with ID " + response.responseBody.id + ""
            });
        } else {
            dispatch({
                type: ADD_HEADER_MESSAGE,
                message: "FAILURE - Session '" + event.form.sessionId + "': " + response.responseBody.message
            });
        }
        event.setFinished();
    },
    expirePublication: (event) => async (dispatch, getState) => {
        dispatch({ type: EXPIRE_PUBLICATION_REQUEST });
        const accessToken = getState().publish.accessToken;
        const message = await publishApiFunctions.expirePublication(event.form.sessionId, event.form.messageId, accessToken, dispatch);
        dispatch({ type: EXPIRE_PUBLICATION_RESPONSE, message });
        dispatch({
            type: ADD_HEADER_MESSAGE,
            message: "Session '" + event.form.sessionId + "': A publication message with ID " + event.form.messageId + " was expired"
        });
        event.setFinished();
    },
    closeSession: (event) => async (dispatch, getState) => {
        dispatch({ type: CLOSE_PUBLICATION_SESSION_REQUEST });
        const accessToken = getState().publish.accessToken;
        const message = await publishApiFunctions.closeSession(event.form.sessionId, accessToken, dispatch);
        dispatch({ type: CLOSE_PUBLICATIONSESSION_RESPONSE, message });
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
