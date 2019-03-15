export const REQUEST_CHANNELS = 'REQUEST_CHANNELS'
export const RECEIVE_CHANNELS = 'RECEIVE_CHANNELS'
export const SET_ACCESS_TOKEN = 'SET_ACCESS_TOKEN'
export const CREATE_CHANNELS_REQUEST = 'CREATE_CHANNELS_REQUEST'
export const CREATE_CHANNELS_RECEIVE = 'CREATE_CHANNELS_RECEIVE'
export const CHANGE_TAB = 'CHANGE_TAB'

const initialState = { channels: [], isLoading: false, accessToken: '', activeTab: 'Get' };
const channelApiFunctions = {
    getChannels: async (accessToken) => {
        const url = 'api/Channels/';
        const response = await fetch(url, {
            method: 'GET',
            headers: {
                "Authorization": accessToken,
                "Content-Type": "application/json"
            }
        });
        const channels = await response.json();
        return channels;
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
        console.log(object['token']);
        dispatch({ type: SET_ACCESS_TOKEN, accessToken: object['token'] });

        dispatch({ type: REQUEST_CHANNELS });
        const channels = await channelApiFunctions.getChannels(object['token']);
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

        const accessToken = getState().channels.accessToken;
        const url = 'api/Channels/';
        const response = await fetch(url, {
            method: 'POST',
            headers: {
                "Authorization": accessToken,
                "Content-Type": "application/json"
            },
            body: JSON.stringify(object)
        });
        await response;
        dispatch({ type: CREATE_CHANNELS_RECEIVE });
    },
    requestChannels: () => async (dispatch, getState) => {
        dispatch({ type: REQUEST_CHANNELS });

        const accessToken = getState().channels.accessToken;
        const channels = await channelApiFunctions.getChannels(accessToken);

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
    return state;
};
