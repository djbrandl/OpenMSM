import React, { Component } from 'react';
import { bindActionCreators } from 'redux';
import { connect } from 'react-redux';
import { actionCreators } from '../store/Publish';
import { Jumbotron, TabContent, TabPane, Nav, NavItem, NavLink, Row, Col, FormGroup, FormText, Label, Input, Button } from 'reactstrap';
import { Formik, Form, Field, ErrorMessage } from 'formik';
import classnames from 'classnames';
import HeaderLogging from './HeaderLogging';

class Publish extends Component {
    render() {
        return (
            <div>
                <HeaderLogging />
                <Jumbotron>
                    <h1 className="display-3">Publisher</h1>
                    <p className="lead">This component demonstrates Publisher session functionality of the REST API.</p>
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
                        <NavLink className={classnames({ active: this.props.activeTab === 'Open' })} onClick={() => { this.props.setActiveTab('Open'); }} href='#'>Open Publication Session</NavLink>
                    </NavItem>
                    <NavItem>
                        <NavLink className={classnames({ active: this.props.activeTab === 'Post' })} onClick={() => { this.props.setActiveTab('Post') }} href='#'>Post Publication Message</NavLink>
                    </NavItem>
                    <NavItem>
                        <NavLink className={classnames({ active: this.props.activeTab === 'Expire' })} onClick={() => { this.props.setActiveTab('Expire') }} href='#'>Expire Publication Message</NavLink>
                    </NavItem>
                    <NavItem>
                        <NavLink className={classnames({ active: this.props.activeTab === 'Close' })} onClick={() => { this.props.setActiveTab('Close') }} href='#'>Close Publication Session</NavLink>
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
                    <TabPane tabId="Expire">
                        <Row>
                            <Col sm="12">
                                <br />
                                {renderExpireMessage(this.props)}
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
            initialValues={{ channelUri: '' }}
            validate={values => {
                let errors = {};
                if (!values.channelUri) {
                    errors.channelUri = 'Required';
                }
                return errors;
            }}
            onSubmit={(values, { resetForm }) => {
                props.openSession({ data: values, setFinished: () => resetForm() });
            }}>

            {({ isSubmitting }) => (
                <Form>
                    <h2>Open a publication session</h2>
                    <FormGroup row>
                        <Label for="channelUri" sm={2}>URI</Label>
                        <Col sm={10}>
                            <Field className="form-control" type="text" name="channelUri" />
                            <ErrorMessage name="channelUri" component="div" />
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
            initialValues={{ uri: '', type: 'Publication', description: '', token: '' }}
            validate={values => {
                let errors = {};
                if (!values.uri) {
                    errors.uri = 'Required';
                }
                return errors;
            }}
            onSubmit={(values, { setSubmitting, resetForm }) => {
                props.createChannel({ data: values, setFinished: () => resetForm() });
            }}>

            {({ isSubmitting }) => (
                <Form>
                    <h2>Create a new channel</h2>
                    <FormGroup row>
                        <Label for="uri" sm={2}>URI</Label>
                        <Col sm={10}>
                            <Field className="form-control" type="text" name="uri" />
                            <ErrorMessage name="uri" component="div" />
                        </Col>
                    </FormGroup>
                    <FormGroup row>
                        <Label for="channelType" sm={2}>Type</Label>
                        <Col sm={10}>
                            <Field component="select" className="form-control" name="type">
                                <option>Publication</option>
                                <option>Request</option>
                            </Field>
                        </Col>
                    </FormGroup>
                    <FormGroup row>
                        <Label for="description" sm={2}>Description</Label>
                        <Col sm={10}>
                            <Field className="form-control" component="textarea" name="description" />
                        </Col>
                    </FormGroup>
                    <FormGroup row>
                        <Label for="token" sm={2}>Security Token</Label>
                        <Col sm={10}>
                            <Field className="form-control" type="password" name="token" />
                            <FormText color="muted">
                                Current front-end implementation of this demonstration only allows for the user to enter just 1 security token when creating a channel. The underlying API driving the behavior <strong>does</strong> allow for multiple security tokens to be assigned to the channel on creation.
                            </FormText>
                        </Col>
                    </FormGroup>
                    <Button type="submit" disabled={isSubmitting}>Submit</Button>
                </Form>
            )}
        </Formik>
    );
}

function renderExpireMessage(props) {
    return (
        <Formik
            initialValues={{ uri: '', type: 'Publication', description: '', token: '' }}
            validate={values => {
                let errors = {};
                if (!values.uri) {
                    errors.uri = 'Required';
                }
                return errors;
            }}
            onSubmit={(values, { setSubmitting, resetForm }) => {
                props.createChannel({ data: values, setFinished: () => resetForm() });
            }}>

            {({ isSubmitting }) => (
                <Form>
                    <h2>Create a new channel</h2>
                    <FormGroup row>
                        <Label for="uri" sm={2}>URI</Label>
                        <Col sm={10}>
                            <Field className="form-control" type="text" name="uri" />
                            <ErrorMessage name="uri" component="div" />
                        </Col>
                    </FormGroup>
                    <FormGroup row>
                        <Label for="channelType" sm={2}>Type</Label>
                        <Col sm={10}>
                            <Field component="select" className="form-control" name="type">
                                <option>Publication</option>
                                <option>Request</option>
                            </Field>
                        </Col>
                    </FormGroup>
                    <FormGroup row>
                        <Label for="description" sm={2}>Description</Label>
                        <Col sm={10}>
                            <Field className="form-control" component="textarea" name="description" />
                        </Col>
                    </FormGroup>
                    <FormGroup row>
                        <Label for="token" sm={2}>Security Token</Label>
                        <Col sm={10}>
                            <Field className="form-control" type="password" name="token" />
                            <FormText color="muted">
                                Current front-end implementation of this demonstration only allows for the user to enter just 1 security token when creating a channel. The underlying API driving the behavior <strong>does</strong> allow for multiple security tokens to be assigned to the channel on creation.
                            </FormText>
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
            initialValues={{ uri: '', type: 'Publication', description: '', token: '' }}
            validate={values => {
                let errors = {};
                if (!values.uri) {
                    errors.uri = 'Required';
                }
                return errors;
            }}
            onSubmit={(values, { setSubmitting, resetForm }) => {
                props.createChannel({ data: values, setFinished: () => resetForm() });
            }}>

            {({ isSubmitting }) => (
                <Form>
                    <h2>Create a new channel</h2>
                    <FormGroup row>
                        <Label for="uri" sm={2}>URI</Label>
                        <Col sm={10}>
                            <Field className="form-control" type="text" name="uri" />
                            <ErrorMessage name="uri" component="div" />
                        </Col>
                    </FormGroup>
                    <FormGroup row>
                        <Label for="channelType" sm={2}>Type</Label>
                        <Col sm={10}>
                            <Field component="select" className="form-control" name="type">
                                <option>Publication</option>
                                <option>Request</option>
                            </Field>
                        </Col>
                    </FormGroup>
                    <FormGroup row>
                        <Label for="description" sm={2}>Description</Label>
                        <Col sm={10}>
                            <Field className="form-control" component="textarea" name="description" />
                        </Col>
                    </FormGroup>
                    <FormGroup row>
                        <Label for="token" sm={2}>Security Token</Label>
                        <Col sm={10}>
                            <Field className="form-control" type="password" name="token" />
                            <FormText color="muted">
                                Current front-end implementation of this demonstration only allows for the user to enter just 1 security token when creating a channel. The underlying API driving the behavior <strong>does</strong> allow for multiple security tokens to be assigned to the channel on creation.
                            </FormText>
                        </Col>
                    </FormGroup>
                    <Button type="submit" disabled={isSubmitting}>Submit</Button>
                </Form>
            )}
        </Formik>
    );
}

export default connect(
    state => state.publish,
    dispatch => bindActionCreators(actionCreators, dispatch)
)(Publish);
