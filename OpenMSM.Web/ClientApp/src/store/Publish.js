import { SET_LAST_API_HEADER, ADD_HEADER_MESSAGE } from './HeaderLoggingOld'
export const OPEN_SESSION_REQUEST = 'OPEN_SESSION_REQUEST'
export const OPEN_SESSION_RECEIVE = 'OPEN_SESSION_RECEIVE'
export const RECEIVE_CHANNELS = 'RECEIVE_CHANNELS'
export const SET_ACCESS_TOKEN = 'SET_ACCESS_TOKEN'
export const DELETE_CHANNEL = 'DELETE_CHANNEL'
export const CREATE_CHANNELS_REQUEST = 'CREATE_CHANNELS_REQUEST'
export const CREATE_CHANNELS_RECEIVE = 'CREATE_CHANNELS_RECEIVE'
export const CHANGE_TAB = 'CHANGE_TAB'

const initialState = {
    accessToken: '',
    activeTab: 'Open',
    headerLogging: {
        activeHeaderTab: 'LastCall',
        lastApiCall: { url: '', details: {}, response: {} },
        messages: []
    }
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
        const url = 'api/session/' + encodeURIComponent(sessionId) + '/publications';
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
        return returnMessage;
    },
    expirePublication: async (sessionId, messageId, accessToken, dispatch) => {
        const url = 'api/session/' + encodeURIComponent(sessionId) + '/publications/' + encodeURIComponent(messageId);
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
        const url = 'api/session/' + encodeURIComponent(sessionId);
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
        const channelUri = event.data.channelUri;
        const publicationSession = await publishApiFunctions.openPublicationSession(channelUri, accessToken, dispatch);
        dispatch({ type: OPEN_SESSION_RECEIVE, publicationSession, channelUri });
        dispatch({
            type: ADD_HEADER_MESSAGE,
            message: "Channel '" + channelUri + "': A publication session was opened with ID '" + publicationSession.id + "'"
        });
        event.setFinished();
    },
    //deleteChannel: (event) => async (dispatch, getState) => {
    //    await publishApiFunctions.deleteChannel(event.data.channelUri, event.data.accessToken, dispatch);
    //    event.setFinished();
    //    dispatch({ type: DELETE_CHANNEL });
    //},
    //createChannel: (event) => async (dispatch, getState) => {
    //    dispatch({ type: CREATE_CHANNELS_REQUEST });
    //    let values = { ...event.data, securityTokens: [] };
    //    values.securityTokens.push({ token: values.token });
    //    delete values.token;
    //    await publishApiFunctions.createChannel(values, dispatch);
    //    event.setFinished();
    //    dispatch({ type: CREATE_CHANNELS_RECEIVE });
    //}

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

    if (action.type === OPEN_SESSION_REQUEST) {
        return {
            ...state,
            isLoading: true
        };
    }

    if (action.type === OPEN_SESSION_RECEIVE) {
        return { ...state };
    }

    if (action.type === CREATE_CHANNELS_REQUEST) {
        return {
            ...state,
            isLoading: true
        };
    }

    if (action.type === CREATE_CHANNELS_RECEIVE) {
        return {
            ...state,
            isLoading: false
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
