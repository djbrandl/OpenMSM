import React, { Component } from 'react';
import { bindActionCreators } from 'redux';
import { connect } from 'react-redux';
import { actionCreators } from '../store/Request';
import { Jumbotron, TabContent, TabPane, Nav, NavItem, NavLink, Row, Col, FormGroup, FormText, Label, Input, Button } from 'reactstrap';
import { Formik, Form, Field, ErrorMessage } from 'formik';
import classnames from 'classnames';
import HeaderLogging from './HeaderLogging';

class Request extends Component {
    render() {
        return (
            <div>
                <HeaderLogging />
                <Jumbotron>
                    <h1 className="display-3">Requester</h1>
                    <p className="lead">This component demonstrates Request session functionality of the REST API.</p>
                    <hr className="my-2" />
                    <Form onSubmit={(e) => { this.props.setAccessToken(e); }}>
                        <FormGroup row>
                            <Label for="token" sm={3}>Set Access Token for all API calls</Label>
                            <Col sm={7}>
                                <Input type="text" name="token" id="token" placeholder="Access Token" />
                            </Col>
                            <Button sm="2">Set</Button>
                        </FormGroup>
                    </Form>
                </Jumbotron>
                <Nav tabs>
                    <NavItem>
                        <NavLink className={classnames({ active: this.props.activeTab === 'Open' })} onClick={() => { this.props.setActiveTab('Open'); }} href='#'>Open Request Session</NavLink>
                    </NavItem>
                    <NavItem>
                        <NavLink className={classnames({ active: this.props.activeTab === 'Post' })} onClick={() => { this.props.setActiveTab('Post') }} href='#'>Post Request Message</NavLink>
                    </NavItem>
                    <NavItem>
                        <NavLink className={classnames({ active: this.props.activeTab === 'Read' })} onClick={() => { this.props.setActiveTab('Read') }} href='#'>Read Response Message</NavLink>
                    </NavItem>
                    <NavItem>
                        <NavLink className={classnames({ active: this.props.activeTab === 'Remove' })} onClick={() => { this.props.setActiveTab('Remove') }} href='#'>Remove Response Message</NavLink>
                    </NavItem>
                    <NavItem>
                        <NavLink className={classnames({ active: this.props.activeTab === 'Close' })} onClick={() => { this.props.setActiveTab('Close') }} href='#'>Close Request Session</NavLink>
                    </NavItem>
                </Nav>
                <TabContent activeTab={this.props.activeTab}>
                    <TabPane tabId="Open">
                        <Row>
                            <Col sm="12">
                                <br />
                                {renderOpenSession(this.props)}
                            </Col>
                        </Row>
                    </TabPane>
                    <TabPane tabId="Post">
                        <Row>
                            <Col sm="12">
                                <br />
                                {renderPostMessage(this.props)}
                            </Col>
                        </Row>
                    </TabPane>
                    <TabPane tabId="Read">
                        <Row>
                            <Col sm="12">
                                <br />
                                {renderReadMessage(this.props)}
                            </Col>
                        </Row>
                    </TabPane>
                    <TabPane tabId="Remove">
                        <Row>
                            <Col sm="12">
                                <br />
                                {renderRemoveMessage(this.props)}
                            </Col>
                        </Row>
                    </TabPane>
                    <TabPane tabId="Close">
                        <Row>
                            <Col sm="12">
                                <br />
                                {renderCloseSession(this.props)}
                            </Col>
                        </Row>
                    </TabPane>
                </TabContent>
            </div>
        );
    }
}

function renderOpenSession(props) {
    return (
        <Formik
            initialValues={{ channelUri: '', listenerURL: '' }}
            validate={values => {
                let errors = {};
                if (!values.channelUri) {
                    errors.channelUri = 'Required';
                }
                return errors;
            }}
            onSubmit={(values, { resetForm }) => {
                props.openSession({ form: values, setFinished: () => resetForm() });
            }}>

            {({ isSubmitting }) => (
                <Form>
                    <h2>Open a request session</h2>
                    <FormGroup row>
                        <Label for="channelUri" sm={2}>Channel URI</Label>
                        <Col sm={10}>
                            <Field className="form-control" type="text" name="channelUri" />
                            <ErrorMessage name="channelUri" component="div" />
                        </Col>
                    </FormGroup>
                    <FormGroup row>
                        <Label for="listenerURL" sm={2}>Listener URL</Label>
                        <Col sm={10}>
                            <Field className="form-control" type="text" name="listenerURL" />
                            <FormText color="muted">
                                This is the URL which will be notified by OpenMSM when this session's outgoing requests have a response.
                            </FormText>
                        </Col>
                    </FormGroup>
                    <Button type="submit" disabled={isSubmitting}>Submit</Button>
                </Form>
            )}
        </Formik>
    );
}

function renderPostMessage(props) {
    return (
        <Formik
            initialValues={{ sessionId: '', message: { type: 'Request', content: '', duration: '', topics: [''] } }}
            validate={values => {
                let errors = {};
                if (!values.sessionId) {
                    errors.sessionId = 'Required';
                }
                return errors;
            }}
            onSubmit={(values, { resetForm }) => {
                props.postRequest({ form: values, setFinished: () => resetForm() });
            }}>

            {({ isSubmitting }) => (
                <Form>
                    <h2>Post a request message</h2>
                    <FormGroup row>
                        <Label for="sessionId" sm={2}>Session ID</Label>
                        <Col sm={10}>
                            <Field className="form-control" type="text" name="sessionId" />
                            <ErrorMessage name="sessionId" component="div" />
                        </Col>
                    </FormGroup>
                    <FormGroup row>
                        <Label for="message.content" sm={2}>Content</Label>
                        <Col sm={10}>
                            <Field className="form-control" component="textarea" name="message.content" />
                        </Col>
                    </FormGroup>
                    <FormGroup row>
                        <Label for="message.duration" sm={2}>Duration</Label>
                        <Col sm={10}>
                            <Field className="form-control" type="text" name="message.duration" />
                            <FormText color="muted">
                                Current implementation requires a valid XML xsd:duration string.
                            </FormText>
                        </Col>
                    </FormGroup>
                    <FormGroup row>
                        <Label for="message.topics[0]" sm={2}>Topic</Label>
                        <Col sm={10}>
                            <Field className="form-control" type="text" name="message.topics[0]" />
                        </Col>
                    </FormGroup>                                  
                    <Button type="submit" disabled={isSubmitting}>Submit</Button>
                </Form>
            )}
        </Formik>
    );
}

function renderReadMessage(props) {
    return (
        <Formik
            initialValues={{ sessionId: '', requestMessageId: '' }}
            validate={values => {
                let errors = {};
                if (!values.sessionId) {
                    errors.sessionId = 'Required';
                }
                if (!values.requestMessageId) {
                    errors.requestMessageId = 'Required';
                }
                return errors;
            }}
            onSubmit={(values, { resetForm }) => {
                props.readResponse({ form: values, setFinished: () => resetForm() });
            }}>

            {({ isSubmitting }) => (
                <Form>
                    <h2>Read a response message</h2>
                    <FormGroup row>
                        <Label for="sessionId" sm={2}>Session ID</Label>
                        <Col sm={10}>
                            <Field className="form-control" type="text" name="sessionId" />
                            <ErrorMessage name="sessionId" component="div" />
                        </Col>
                    </FormGroup>
                    <FormGroup row>
                        <Label for="requestMessageId" sm={2}>Request Message ID</Label>
                        <Col sm={10}>
                            <Field className="form-control" type="text" name="requestMessageId" />
                            <ErrorMessage name="requestMessageId" component="div" />
                        </Col>
                    </FormGroup>
                    <Button type="submit" disabled={isSubmitting}>Submit</Button>
                </Form>
            )}
        </Formik>
    );
}

function renderRemoveMessage(props) {
    return (
        <Formik
            initialValues={{ sessionId: '', requestMessageId: '' }}
            validate={values => {
                let errors = {};
                if (!values.sessionId) {
                    errors.sessionId = 'Required';
                }
                if (!values.requestMessageId) {
                    errors.requestMessageId = 'Required';
                }
                return errors;
            }}
            onSubmit={(values, { resetForm }) => {
                props.removeResponse({ form: values, setFinished: () => resetForm() });
            }}>

            {({ isSubmitting }) => (
                <Form>
                    <h2>Remove a response message</h2>
                    <FormGroup row>
                        <Label for="sessionId" sm={2}>Session ID</Label>
                        <Col sm={10}>
                            <Field className="form-control" type="text" name="sessionId" />
                            <ErrorMessage name="sessionId" component="div" />
                        </Col>
                    </FormGroup>
                    <FormGroup row>
                        <Label for="requestMessageId" sm={2}>Request Message ID</Label>
                        <Col sm={10}>
                            <Field className="form-control" type="text" name="requestMessageId" />
                            <ErrorMessage name="requestMessageId" component="div" />
                        </Col>
                    </FormGroup>
                    <Button type="submit" disabled={isSubmitting}>Submit</Button>
                </Form>
            )}
        </Formik>
    );
}

function renderCloseSession(props) {
    return (
        <Formik
            initialValues={{ sessionId: '' }}
            validate={values => {
                let errors = {};
                if (!values.sessionId) {
                    errors.sessionId = 'Required';
                }
                return errors;
            }}
            onSubmit={(values, { resetForm }) => {
                props.closeSession({ form: values, setFinished: () => resetForm() });
            }}>

            {({ isSubmitting }) => (
                <Form>
                    <h2>Close a request session</h2>
                    <FormGroup row>
                        <Label for="sessionId" sm={2}>Session ID</Label>
                        <Col sm={10}>
                            <Field className="form-control" type="text" name="sessionId" />
                            <ErrorMessage name="sessionId" component="div" />
                        </Col>
                    </FormGroup>
                    <Button type="submit" disabled={isSubmitting}>Submit</Button>
                </Form>
            )}
        </Formik>
    );
}

export default connect(
    state => state.request,
    dispatch => bindActionCreators(actionCreators, dispatch)
)(Request);
