import React, { Component } from 'react';
import { bindActionCreators } from 'redux';
import { connect } from 'react-redux';
import { actionCreators } from '../store/Channels';


class FetchData extends Component {
    componentDidMount() {
        // This method is called when the component is first added to the document
        this.ensureDataFetched();
    }
    componentDidUpdate() {
        // This method is called when the route parameters change
        this.ensureDataFetched();
    }
    ensureDataFetched() {
        this.props.requestChannels();
    }
    render() {
        return (
            <div>
                <h1>Channels</h1>
                <hr/>
                <p>This component demonstrates fetching channels from the REST API.</p>
                {renderChannels(this.props)}
            </div>
        );
    }
}

function renderChannels(props) {
    return (
        <table className='table table-striped'>
            <thead>
                <tr>
                    <th>URI</th>
                    <th>Type</th>
                    <th>Topics</th>
                </tr>
            </thead>
            <tbody>
                {props.channels.map(channel =>
                    <tr key={channel.ChannelUri}>
                        <td>{channel.uri}</td>
                        <td>{channel.type}</td>
                        <td>{channel.tokens}</td>
                            
                    </tr>
                )}
            </tbody>
        </table>
    );
}

export default connect(
    state => state.channels,
    dispatch => bindActionCreators(actionCreators, dispatch)
)(FetchData);
