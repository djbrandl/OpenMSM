export const ADD_HEADER_MESSAGE = 'ADD_HEADER_MESSAGE'
export const CHANGE_HEADER_TAB = 'CHANGE_HEADER_TAB'
export const SET_LAST_API_HEADER = 'SET_LAST_API_HEADER'
export const TOGGLE_SLIDE_OUT = 'TOGGLE_SLIDE_OUT'

const initialState = {
    activeHeaderTab: 'LastCall',
    slideOut: false,
    lastApiCall: { url: '', details: {}, response: {} },
    messages: []
};

export const actionCreators = {
    setActiveTab: (tab) => async (dispatch, getState) => {
        dispatch({ type: CHANGE_HEADER_TAB, tab: tab });
    },
    toggleSlideOut: (tab) => async (dispatch, getState) => {
        dispatch({ type: TOGGLE_SLIDE_OUT });
    }
};

// reducers update state
// reducers cannot contain asynchronous code
// do not modify the original state, but make sure you copy the state
export const reducer = (state, action) => {
    state = state || initialState;

    if (action.type === CHANGE_HEADER_TAB) {
        return {
            ...state,
            activeHeaderTab: action.tab
        };
    }
    if (action.type === TOGGLE_SLIDE_OUT) {
        return {
            ...state,
            slideOut: !state.slideOut
        };
    }

    if (action.type === ADD_HEADER_MESSAGE) {
        const messages = [...state.messages];
        messages.push(action.message);
        messages.reverse();
        return {
            ...state,
            messages: messages
        };
    }

    if (action.type === SET_LAST_API_HEADER) {
        return {
            ...state,
            lastApiCall: {
                url: action.lastApiCallUrl,
                details: action.lastApiCallDetails,
                response: action.lastApiResponse,
            }
        };
    }
    return state;
};
