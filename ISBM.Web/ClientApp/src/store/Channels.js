//import { reset } from 'redux-form';

export const REQUEST_CHANNELS = 'REQUEST_CHANNELS'
export const RECEIVE_CHANNELS = 'RECEIVE_CHANNELS'
export const SET_ACCESS_TOKEN = 'SET_ACCESS_TOKEN'
export const CREATE_CHANNELS_REQUEST = 'CREATE_CHANNELS_REQUEST'
export const CREATE_CHANNELS_RECEIVE = 'CREATE_CHANNELS_RECEIVE'
export const CHANGE_TAB = 'CHANGE_TAB'
export const TOGGLE_API = 'TOGGLE_API'
export const SET_LAST_API = 'SET_LAST_API'

const initialState = {
    channels: [], isLoading: false, accessToken: '', activeTab: 'Get', showApi: false, lastApiCallUrl: '', lastApiCallDetails: {}, lastApiResponse: {}
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
        const url = 'api/Channels/';
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
            dispatch({ type: SET_LAST_API, lastApiCallUrl: url, lastApiCallDetails: options, lastApiResponse: buildResponse(response) });
            return channels;
        }
        catch (e) {

        }
    },
    createChannel: async (object, dispatch) => {
        const url = 'api/Channels/';
        const options = {
            method: 'POST',
            headers: {
                "Content-Type": "application/json"
            },
            body: JSON.stringify(object)
        };
        const response = await fetch(url, options);
        dispatch({ type: SET_LAST_API, lastApiCallUrl: url, lastApiCallDetails: options, lastApiResponse: buildResponse(response) });
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

        dispatch({ type: REQUEST_CHANNELS });
        const channels = await channelApiFunctions.getChannels(object['token'], dispatch);
        dispatch({ type: RECEIVE_CHANNELS, channels });
    },
    setActiveTab: (tab) => async (dispatch, getState) => {
        dispatch({ type: CHANGE_TAB, tab: tab });
    },
    createChannel: (event) => async (dispatch, getState) => {
        dispatch({ type: CREATE_CHANNELS_REQUEST });

        event.preventDefault();
        const data = new FormData(event.target);
        var object = {};
        data.forEach(function (value, key) {
            if (key === 'securityToken') {
                object['securityTokens'] = [];
                if (value !== '') {
                    object['securityTokens'].push({ "token": value });
                }
            } else {
                object[key] = value;
            }
        });

        await channelApiFunctions.createChannel(object, dispatch);
        dispatch({ type: CREATE_CHANNELS_RECEIVE });
        //dispatch(reset('createChannel'));
    },
    requestChannels: () => async (dispatch, getState) => {
        dispatch({ type: REQUEST_CHANNELS });

        const accessToken = getState().channels.accessToken;
        const channels = await channelApiFunctions.getChannels(accessToken, dispatch);

        dispatch({ type: RECEIVE_CHANNELS, channels });
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
        console.log(action.lastApiResponse);
        return {
            ...state,
            lastApiCallUrl: action.lastApiCallUrl,
            lastApiCallDetails: action.lastApiCallDetails,
            lastApiResponse: action.lastApiResponse,
        };
    }
    return state;
};
