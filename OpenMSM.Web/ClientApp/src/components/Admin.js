import React, { Component } from 'react';
import { bindActionCreators } from 'redux';
import { connect } from 'react-redux';
import { actionCreators } from '../store/Admin';
import LogMessage from './LogMessage'

class Admin extends Component {
    componentDidMount() {
        this.props.startSignalR();
    }
    render() {
        return (
            <div>
                <h1>Admin</h1>
                {[...this.props.logMessages].reverse().map((message, index) => (
                    <LogMessage key={index} data={message} index={index}/>
                ))}
            </div>
        );
    }
}


export default connect(
    state => state.admin,
    dispatch => bindActionCreators(actionCreators, dispatch)
)(Admin);
