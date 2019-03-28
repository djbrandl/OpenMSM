import React, { Component } from 'react';
import { bindActionCreators } from 'redux';
import { connect } from 'react-redux';
import { actionCreators } from '../store/Subscribe';
import { Jumbotron, TabContent, TabPane, Nav, NavItem, NavLink, Row, Col, FormGroup, FormText, Label, Input, Button, InputGroup, InputGroupAddon } from 'reactstrap';
import { Formik, Form, Field, FieldArray, ErrorMessage } from 'formik';
import classnames from 'classnames';
import HeaderLogging from './HeaderLogging';

class Subscribe extends Component {
    render() {
        return (
            <div>
                <HeaderLogging />
                <Jumbotron>
                    <h1 className="display-3">Subscriber</h1>
                    <p className="lead">This component demonstrates Subscriber session functionality of the REST API.</p>
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
                        <NavLink className={classnames({ active: this.props.activeTab === 'Open' })} onClick={() => { this.props.setActiveTab('Open'); }} href='#'>Open Subscription Session</NavLink>
                    </NavItem>
                    <NavItem>
                        <NavLink className={classnames({ active: this.props.activeTab === 'Read' })} onClick={() => { this.props.setActiveTab('Read') }} href='#'>Read Publication Message</NavLink>
                    </NavItem>
                    <NavItem>
                        <NavLink className={classnames({ active: this.props.activeTab === 'Remove' })} onClick={() => { this.props.setActiveTab('Remove') }} href='#'>Remove Publication Message</NavLink>
                    </NavItem>
                    <NavItem>
                        <NavLink className={classnames({ active: this.props.activeTab === 'Close' })} onClick={() => { this.props.setActiveTab('Close') }} href='#'>Close Subscription Session</NavLink>
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
            initialValues={{ channelUri: '', session: { type: 'PublicationConsumer', listenerUrl: '', xPathExpression: '', topics: [''], xPathNamespaces: [] } }}
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

            {({ values, isSubmitting }) => (
                <Form>
                    <h2>Open a subscription session</h2>
                    <FormGroup row>
                        <Label for="channelUri" sm={2}>Channel URI</Label>
                        <Col sm={10}>
                            <Field className="form-control" type="text" name="channelUri" />
                            <ErrorMessage name="channelUri" component="div" />
                        </Col>
                    </FormGroup>
                    <FormGroup row>
                        <Label for="session.listenerUrl" sm={2}>Listener URL</Label>
                        <Col sm={10}>
                            <Field className="form-control" type="text" name="session.listenerUrl" />
                            <FormText color="muted">
                                This is the HTTP URL which the server will notify via an HTTP POST when a message matching the topic and/or XPath criteria is published.
                            </FormText>
                        </Col>
                    </FormGroup>
                    <FormGroup row>
                        <Label for="session.xPathExpression" sm={2}>XPathExpression</Label>
                        <Col sm={10}>
                            <Field className="form-control" type="text" name="session.xPathExpression" />
                        </Col>
                    </FormGroup>
                    <FieldArray
                        name="session.topics"
                        render={arrayHelpers => (
                            <FormGroup row>
                                <Label sm="2">Topics</Label>
                                <Col sm="10">
                                    {values.session.topics.map((topic, index) => (
                                        <FormGroup row key={index} >
                                            <InputGroup size="md">
                                                <Field className='form-control' name={`session.topics[${index}]`} />
                                                <InputGroupAddon addonType="append"><Button onClick={() => arrayHelpers.push('')}>&nbsp;+&nbsp;</Button></InputGroupAddon>
                                                {index > 0 ?
                                                    <InputGroupAddon addonType="append"><Button onClick={() => arrayHelpers.remove(index)}>&nbsp;-&nbsp;</Button></InputGroupAddon>
                                                    : ('')}
                                            </InputGroup>
                                        </FormGroup>
                                    ))}
                                </Col>
                            </FormGroup>
                        )} />
                    <FieldArray
                        name="session.xPathNamespaces"
                        render={arrayHelpers => (
                            <FormGroup row>
                                <Label sm="2">XPath Namespaces</Label>
                                <Col sm="10">
                                    {!values.session.xPathNamespaces || values.session.xPathNamespaces.length === 0 ?
                                        <FormGroup row>
                                            <Button onClick={() => arrayHelpers.push({ prefix: '', namespace: '' })}>Add XPath Namespace</Button>
                                        </FormGroup>
                                        : ('')}
                                    {values.session.xPathNamespaces.map((xpns, index) => (
                                        <FormGroup row key={index}>
                                            <InputGroup size="md">
                                                <InputGroupAddon addonType="prepend">Prefix:</InputGroupAddon>
                                                <Field className='form-control' name={`session.xPathNamespaces[${index}].prefix`} />
                                            </InputGroup>
                                            <InputGroup>
                                                <InputGroupAddon addonType="prepend">Namespace:</InputGroupAddon>
                                                <Field className='form-control' name={`session.xPathNamespaces[${index}].namespace`} />
                                                <InputGroupAddon addonType="append"><Button onClick={() => arrayHelpers.push({ prefix: '', namespace: '' })}>&nbsp;+&nbsp;</Button></InputGroupAddon>
                                                <InputGroupAddon addonType="append"><Button onClick={() => arrayHelpers.remove(index)}>&nbsp;-&nbsp;</Button></InputGroupAddon>
                                            </InputGroup>
                                        </FormGroup>
                                    ))}
                                </Col>
                            </FormGroup>
                        )} />
                    <Button type="submit" disabled={isSubmitting}>Submit</Button>
                </Form>
            )}
        </Formik>
    );
}

function renderReadMessage(props) {
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
                props.readPublication({ form: values, setFinished: () => resetForm() });
            }}>

            {({ isSubmitting }) => (
                <Form>
                    <h2>Read a publication message</h2>
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

function renderRemoveMessage(props) {
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
                props.removePublication({ form: values, setFinished: () => resetForm() });
            }}>

            {({ isSubmitting }) => (
                <Form>
                    <h2>Remove a publication message</h2>
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
                    <h2>Close a subscription session</h2>
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
    state => state.subscribe,
    dispatch => bindActionCreators(actionCreators, dispatch)
)(Subscribe);
