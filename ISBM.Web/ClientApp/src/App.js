import React from 'react';
import { Route } from 'react-router';
import Layout from './components/Layout';
import Home from './components/Home';
import Channels from './components/Channels';

export default () => (
  <Layout>
    <Route exact path='/' component={Home} />
    <Route path='/channels' component={Channels} />
  </Layout>
);
