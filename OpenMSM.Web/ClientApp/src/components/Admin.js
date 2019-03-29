import React, { Component } from 'react';
import { bindActionCreators } from 'redux';
import { connect } from 'react-redux';
import { actionCreators } from '../store/Admin';
import { Jumbotron, TabContent, TabPane, Nav, NavItem, NavLink, Row, Col, FormGroup, Label, Button, FormText } from 'reactstrap';
import { Formik, Form, Field } from 'formik';
import LogMessage from './LogMessage'
import classnames from 'classnames';

class Admin extends Component {
    componentDidMount() {
        this.props.startSignalR();
        this.props.getConfiguration();
    }
    render() {
        return (
            <div>
                <Jumbotron>
                    <h1 className="display-3">Admin</h1>
                    <p className="lead">This page encompasses some Admin functionality for debugging and maintenance.</p>
                </Jumbotron>
                <Nav tabs>
                    <NavItem>
                        <NavLink className={classnames({ active: this.props.activeTab === 'Log' })} onClick={() => { this.props.setActiveTab('Log'); }} href='#'>Live Message Logging</NavLink>
                    </NavItem>
                    <NavItem>
                        <NavLink className={classnames({ active: this.props.activeTab === 'Sessions' })} onClick={() => { this.props.setActiveTab('Sessions') }} href='#'>Sessions by Last Activity</NavLink>
                    </NavItem>
                    <NavItem>
                        <NavLink className={classnames({ active: this.props.activeTab === 'Options' })} onClick={() => { this.props.setActiveTab('Options') }} href='#'>Security Options</NavLink>
                    </NavItem>
                </Nav>
                <TabContent activeTab={this.props.activeTab}>
                    <TabPane tabId="Log">
                        <Row>
                            <Col sm="12">
                                <br />
                                {[...this.props.logMessages].reverse().map((message, index) => (
                                    <LogMessage key={index} data={message} index={index} />
                                ))}
                            </Col>
                        </Row>
                    </TabPane>
                    <TabPane tabId="Sessions">
                        <Row>
                            <Col sm="12">
                                <br />
                                Kill orphaned sessions (show sessions ordered by most recent activity descending)
                            </Col>
                        </Row>
                    </TabPane>
                    <TabPane tabId="Options">
                        <Row>
                            <Col sm="12">
                                <br />
                                Control security options
                                <Formik
                                    enableReinitialize
                                    initialValues={{ storeLogMessages: this.props.storeLogMessages, numberOfMessagesToStore: this.props.numberOfMessagesToStore }}
                                    onSubmit={(values, { setSubmitting }) => {
                                        this.props.updateConfiguration({ values, setFinished: () => setSubmitting(false) });
                                    }}>

                                    {({ isSubmitting }) => (
                                        <Form>
                                            <h4>Update Logging Configuration</h4>
                                            <FormGroup row>
                                                <Label for="storeLogMessages" sm={2}>Store Log Messages?</Label>
                                                <Col sm={10}>
                                                    <Field className="form-control" type="checkbox" checked={this.props.storeLogMessages ? "checked" : false} name="storeLogMessages" />
                                                </Col>
                                            </FormGroup>
                                            <FormGroup row>
                                                <Label for="numberOfMessagesToStore" sm={2}>Number of Messages to Store</Label>
                                                <Col sm={10}>
                                                    <Field className="form-control" type="text" name="numberOfMessagesToStore" />
                                                </Col>
                                            </FormGroup>
                                            <FormText>This for allows for the storing of all API message requests and responses in the database in their raw form. Please only turn this on for debugging purposes. This data is only viewable in the database "LogApiMessages" table.</FormText>
                                            <br />
                                            <Button type="submit" disabled={isSubmitting}>Submit</Button>
                                        </Form>
                                    )}
                                </Formik>
                                <ul>
                                    <li>Disable remote channel creation</li>
                                    <li>Enable/disable white listing of clients by IP or potentially MAC address (if available)</li>
                                </ul>
                            </Col>
                        </Row>
                    </TabPane>
                </TabContent>
            </div>
        );
    }
}


export default connect(
    state => state.admin,
    dispatch => bindActionCreators(actionCreators, dispatch)
)(Admin);
