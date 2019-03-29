const signalR = require('@aspnet/signalr');

export const START_SIGNALR = 'START_SIGNALR'
export const SIGNALR_CONNECTED = 'SIGNALR_CONNECTED'
export const MESSAGE_RECEIVED = 'MESSAGE_RECEIVED'
export const GET_CONFIGURATION = 'GET_CONFIGURATION'
export const UPDATE_CONFIGURATION = 'UPDATE_CONFIGURATION'
export const CHANGE_TAB = 'CHANGE_TAB'

const initialState = {
    activeTab: 'Log',
    logMessages: [],
    storeLogMessages: true,
    NumberOfMessagesToStore: 2000
};

const adminApiFunctions = {
    getConfiguration: async () => {
        const url = 'api/admin/configuration';
        const options = {
            method: 'GET',
            headers: {
                "Content-Type": "application/json"
            }
        };
        const response = await fetch(url, options);
        const returnMessage = await response.json();
        return returnMessage;
    },
    updateConfiguration: async (configuration) => {
        const url = 'api/admin/configuration';
        const options = {
            method: 'POST',
            headers: {
                "Content-Type": "application/json"
            },
            body: JSON.stringify(configuration)
        };
        const response = await fetch(url, options);
        const returnMessage = await response.json();
        return { responseData: response, responseBody: returnMessage };
    },
}

export const actionCreators = {
    setActiveTab: (tab) => async (dispatch, getState) => {
        dispatch({ type: CHANGE_TAB, tab: tab });
    },
    startSignalR: () => async (dispatch) => {
        dispatch({ type: START_SIGNALR });
        const connection = new signalR.HubConnectionBuilder().withUrl('/admin/hub').build();
        connection.on("ActionOccurred", data => {
            dispatch({ type: MESSAGE_RECEIVED, message: data });
        });
        await connection.start();
        dispatch({ type: SIGNALR_CONNECTED });
    },
    getConfiguration: () => async (dispatch, getState) => {
        const configuration = await adminApiFunctions.getConfiguration();
        dispatch({ type: GET_CONFIGURATION, configuration });
    },
    updateConfiguration: (form) => async (dispatch, getState) => {
        const configuration = form.values;
        await adminApiFunctions.updateConfiguration(configuration);
        dispatch({ type: GET_CONFIGURATION, configuration });
    }
};

// reducers update state
// reducers cannot contain asynchronous code
// do not modify the original state, but make sure you copy the state
export const reducer = (state, action) => {
    state = state || initialState;

    if (action.type === CHANGE_TAB) {
        return {
            ...state,
            activeTab: action.tab
        };
    }
    if (action.type === GET_CONFIGURATION) {
        return {
            ...state,
            activeTab: action.tab
        };
    }

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
