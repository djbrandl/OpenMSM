import React, { Component } from 'react';
import { connect } from 'react-redux';
import { ListGroup, ListGroupItem, UncontrolledCollapse, Button } from 'reactstrap';

class Messages extends Component {
    render() {
        return (
            <div>
                <Button id='toggle-messages'>Toggle Messages</Button>
                <UncontrolledCollapse toggler='#toggle-messages'>
                    <ListGroup>
                        {this.props.messages.map((message, index) => (
                            <ListGroupItem key={index}>{message}</ListGroupItem>
                        ))}
                    </ListGroup>
                </UncontrolledCollapse>
            </div>
        );
    }
}

export default connect()(Messages);
