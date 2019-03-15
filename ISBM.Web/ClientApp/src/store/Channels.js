export const REQUEST_CHANNELS = 'REQUEST_CHANNELS'
export const RECEIVE_CHANNELS = 'RECEIVE_CHANNELS'

const initialState = { channels: [], isLoading: false };


export const actionCreators = {
    requestChannels: () => async (dispatch, getState) => {
        dispatch({ type: REQUEST_CHANNELS });

        const url = 'api/Channels/';
        const response = await fetch(url);
        const channels = await response.json();

        dispatch({ type: RECEIVE_CHANNELS, channels });
    }
};

// reducers update state
// reducers cannot contain asynchronous code
// do not modify the original state, but make sure you copy the state
export const reducer = (state, action) => {
    state = state || initialState;

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

    return state;
};
