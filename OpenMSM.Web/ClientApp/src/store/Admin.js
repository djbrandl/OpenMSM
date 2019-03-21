
const signalR = require('@aspnet/signalr');

export const START_SIGNALR = 'START_SIGNALR'
export const SIGNALR_CONNECTED = 'SIGNALR_CONNECTED'
export const MESSAGE_RECEIVED = 'MESSAGE_RECEIVED'

const initialState = {
    logMessages: []
};

export const actionCreators = {
    startSignalR: () => async (dispatch) => {
        dispatch({ type: START_SIGNALR });
        const connection = new signalR.HubConnectionBuilder().withUrl('/admin/hub').build();
        connection.on("ActionOccurred", data => {
            dispatch({ type: MESSAGE_RECEIVED, message: data });
        });
        await connection.start();
        dispatch({ type: SIGNALR_CONNECTED });
    }
};

// reducers update state
// reducers cannot contain asynchronous code
// do not modify the original state, but make sure you copy the state
export const reducer = (state, action) => {
    state = state || initialState;

    if (action.type === MESSAGE_RECEIVED) {
        const newMessages = [...state.logMessages];
        newMessages.push(action.message);
        return {
            ...state,
            logMessages: newMessages 
        };
    }
    return state;
};
