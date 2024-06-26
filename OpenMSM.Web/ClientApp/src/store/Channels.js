﻿import { SET_LAST_API_HEADER, ADD_HEADER_MESSAGE } from './HeaderLogging'
export const REQUEST_CHANNELS = 'REQUEST_CHANNELS'
export const RECEIVE_CHANNELS = 'RECEIVE_CHANNELS'
export const SET_ACCESS_TOKEN = 'SET_ACCESS_TOKEN'
export const ADD_ACCESS_TOKEN = 'ADD_ACCESS_TOKEN'
export const REMOVE_ACCESS_TOKEN = 'REMOVE_ACCESS_TOKEN'
export const DELETE_CHANNEL = 'DELETE_CHANNEL'
export const CREATE_CHANNELS_REQUEST = 'CREATE_CHANNELS_REQUEST'
export const CREATE_CHANNELS_RECEIVE = 'CREATE_CHANNELS_RECEIVE'
export const CHANGE_TAB = 'CHANGE_TAB'
export const TOGGLE_API = 'TOGGLE_API'
export const SET_LAST_API = 'SET_LAST_API'

const initialState = {
    channels: [],
    isLoading: false,
    accessToken: '',
    activeTab: 'Get',
    showApi: false,
    lastApiCallUrl: '',
    lastApiCallDetails: {},
    lastApiResponse: {}
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

const channelApiFunctions = {
    getChannels: async (accessToken, dispatch) => {
        const url = 'api/channels/';
        const options = {
            method: 'GET',
            headers: {
                "Authorization": accessToken,
                "Content-Type": "application/json"
            }
        };
        try {
            const response = await fetch(url, options);
            const channels = await response.json();
            if (dispatch) {
                dispatch({ type: SET_LAST_API_HEADER, lastApiCallUrl: url, lastApiCallDetails: options, lastApiResponse: buildResponse(response) });
            }
            return channels;
        }
        catch (e) {

        }
    },
    createChannel: async (object, dispatch) => {
        const url = 'api/channels/';
        const options = {
            method: 'POST',
            headers: {
                "Content-Type": "application/json"
            },
            body: JSON.stringify(object)
        };
        const response = await fetch(url, options);
        if (dispatch) {
            dispatch({ type: SET_LAST_API_HEADER, lastApiCallUrl: url, lastApiCallDetails: options, lastApiResponse: buildResponse(response) });
        }
        return response;
    },
    addSecurityTokens: async (channelUri, accessToken, object, dispatch) => {
        const url = 'api/channels/' + encodeURIComponent(channelUri) + '/security-tokens';
        console.log(url);
        const options = {
            method: 'POST',
            headers: {
                "Authorization": accessToken,
                "Content-Type": "application/json"
            },
            body: JSON.stringify(object)
        };
        const response = await fetch(url, options);
        if (dispatch) {
            dispatch({ type: SET_LAST_API_HEADER, lastApiCallUrl: url, lastApiCallDetails: options, lastApiResponse: buildResponse(response) });
        }
        return response;
    },
    removeSecurityTokens: async (channelUri, accessToken, object, dispatch) => {
        const url = 'api/channels/' + encodeURIComponent(channelUri) + '/security-tokens';
        const options = {
            method: 'DELETE',
            headers: {
                "Authorization": accessToken,
                "Content-Type": "application/json"
            },
            body: JSON.stringify(object)
        };
        const response = await fetch(url, options);
        if (dispatch) {
            dispatch({ type: SET_LAST_API_HEADER, lastApiCallUrl: url, lastApiCallDetails: options, lastApiResponse: buildResponse(response) });
        }
        return response;
    },
    deleteChannel: async (channelUri, accessToken, dispatch) => {
        const url = 'api/channels/' + encodeURIComponent(channelUri);
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
    toggleApi: () => async (dispatch) => {
        dispatch({ type: TOGGLE_API });
    },
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
    getChannels: () => async (dispatch, getState) => {
        dispatch({ type: REQUEST_CHANNELS });
        const channels = await channelApiFunctions.getChannels(getState().channels.accessToken, dispatch);
        dispatch({ type: RECEIVE_CHANNELS, channels });
    },
    addSecurityTokens: (event) => async (dispatch, getState) => {
        await channelApiFunctions.addSecurityTokens(event.data.channelUri, getState().channels.accessToken, event.data.securityTokens, dispatch);
        dispatch({
            type: ADD_HEADER_MESSAGE, message: "Channel '" + event.data.channelUri + "': Added " + event.data.securityTokens.length + " security tokens to channel."
        });
        event.setFinished();
        dispatch({ type: ADD_ACCESS_TOKEN });
    },
    removeSecurityTokens: (event) => async (dispatch, getState) => {
        await channelApiFunctions.removeSecurityTokens(event.data.channelUri, getState().channels.accessToken, event.data.securityTokens, dispatch);
        dispatch({
            type: ADD_HEADER_MESSAGE, message: "Channel '" + event.data.channelUri + "': Removed security tokens from channel."
        });
        event.setFinished();
        dispatch({ type: REMOVE_ACCESS_TOKEN });
    },
    deleteChannel: (event) => async (dispatch, getState) => {
        await channelApiFunctions.deleteChannel(event.data.channelUri, getState().channels.accessToken, dispatch);
        dispatch({
            type: ADD_HEADER_MESSAGE, message: "Channel '" + event.data.channelUri + "': Deleted channel."
        });
        event.setFinished();
        dispatch({ type: DELETE_CHANNEL });
    },
    createChannel: (event) => async (dispatch, getState) => {
        dispatch({ type: CREATE_CHANNELS_REQUEST });
        let values = { ...event.data, securityTokens: [] };
        values.securityTokens.push({ token: values.token });
        delete values.token;
        await channelApiFunctions.createChannel(values, dispatch);
        dispatch({
            type: ADD_HEADER_MESSAGE, message: "Channel '" + values.uri + "': Created channel."
        });
        event.setFinished();
        dispatch({ type: CREATE_CHANNELS_RECEIVE });
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

    if (action.type === REQUEST_CHANNELS) {
        return {
            ...state,
            isLoading: true
        };
    }

    if (action.type === RECEIVE_CHANNELS) {
        return {
            ...state,
            channels: action.channels,
            isLoading: false
        };
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

    if (action.type === TOGGLE_API) {
        return {
            ...state,
            showApi: !state.showApi
        };
    }

    if (action.type === SET_LAST_API) {
        return {
            ...state,
            lastApiCallUrl: action.lastApiCallUrl,
            lastApiCallDetails: action.lastApiCallDetails,
            lastApiResponse: action.lastApiResponse,
        };
    }
    return state;
};
