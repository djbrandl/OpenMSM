import { applyMiddleware, combineReducers, compose, createStore } from 'redux';
import thunk from 'redux-thunk';
import { routerReducer, routerMiddleware } from 'react-router-redux';
import * as Channels from './Channels';
import * as Admin from './Admin';
import * as HeaderLogging from './HeaderLogging';
import * as Publish from './Publish';
import * as Subscribe from './Subscribe';
import * as Request from './Request';
import * as Respond from './Respond';

export default function configureStore(history, initialState) {
    const reducers = {
        channels: Channels.reducer,
        admin: Admin.reducer,
        headerLogging: HeaderLogging.reducer,
        publish: Publish.reducer,
        subscribe: Subscribe.reducer,
        request: Request.reducer,
        respond: Respond.reducer
    };

    const middleware = [
        thunk,
        routerMiddleware(history)
    ];

    // In development, use the browser's Redux dev tools extension if installed
    const enhancers = [];
    const isDevelopment = process.env.NODE_ENV === 'development';
    if (isDevelopment && typeof window !== 'undefined' && window.devToolsExtension) {
        enhancers.push(window.devToolsExtension());
    }

    const rootReducer = combineReducers({
        ...reducers,
        routing: routerReducer
    });

    return createStore(
        rootReducer,
        initialState,
        compose(applyMiddleware(...middleware), ...enhancers)
    );
}
