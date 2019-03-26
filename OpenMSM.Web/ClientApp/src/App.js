import React from 'react';
import { Route } from 'react-router';
import Layout from './components/Layout';
import Home from './components/Home';
import Channels from './components/Channels';
import Admin from './components/Admin';
import Publish from './components/Publish';
import Subscribe from './components/Subscribe';

export default () => (
    <Layout>
        <Route exact path='/' component={Home} />
        <Route path='/channels' component={Channels} />
        <Route path='/admin' component={Admin} />
        <Route path='/publish' component={Publish} />
        <Route path='/subscribe' component={Subscribe} />
    </Layout>
);
