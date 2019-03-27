import React, { Component } from 'react';
import { bindActionCreators } from 'redux';
import { connect } from 'react-redux';
import { actionCreators } from '../store/Publish';
import { Jumbotron, TabContent, TabPane, Nav, NavItem, NavLink, Row, Col, FormGroup, FormText, Label, Input, Button, InputGroup, InputGroupAddon } from 'reactstrap';
import { Formik, Form, Field, FieldArray, ErrorMessage } from 'formik';
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
                props.openSession({ form: values, setFinished: () => resetForm() });
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
            initialValues={{ sessionId: '', message: { type: 'Publication', content: '', duration: '', topics: [''] } }}
            validate={values => {
                let errors = {};
                if (!values.sessionId) {
                    errors.sessionId = 'Required';
                }
                return errors;
            }}
            onSubmit={(values, { resetForm }) => {
                props.postPublication({ form: values, setFinished: () => resetForm() });
            }}>

            {({ values, isSubmitting }) => (
                <Form>
                    <h2>Post a publication message</h2>
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
                    <FieldArray
                        name="message.topics"
                        render={arrayHelpers => (
                            <FormGroup row>
                                <Label sm="2">Topics</Label>
                                <Col sm="10">
                                    {values.message.topics.map((topic, index) => (
                                        <FormGroup row key={index} >
                                            <InputGroup size="md">
                                                <Field className='form-control' name={`message.topics[${index}]`} />
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
                    <Button type="submit" disabled={isSubmitting}>Submit</Button>
                </Form>
            )}
        </Formik>
    );
}

function renderExpireMessage(props) {
    return (
        <Formik
            initialValues={{ sessionId: '', messageId: '' }}
            validate={values => {
                let errors = {};
                if (!values.sessionId) {
                    errors.sessionId = 'Required';
                }
                if (!values.messageId) {
                    errors.messageId = 'Required';
                }
                return errors;
            }}
            onSubmit={(values, { resetForm }) => {
                props.expirePublication({ form: values, setFinished: () => resetForm() });
            }}>

            {({ isSubmitting }) => (
                <Form>
                    <h2>Expire a publication message</h2>
                    <FormGroup row>
                        <Label for="sessionId" sm={2}>Session ID</Label>
                        <Col sm={10}>
                            <Field className="form-control" type="text" name="sessionId" />
                            <ErrorMessage name="sessionId" component="div" />
                        </Col>
                    </FormGroup>
                    <FormGroup row>
                        <Label for="messageId" sm={2}>Message ID</Label>
                        <Col sm={10}>
                            <Field className="form-control" type="text" name="messageId" />
                            <ErrorMessage name="messageId" component="div" />
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
                    <h2>Close a publication session</h2>
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
    state => state.publish,
    dispatch => bindActionCreators(actionCreators, dispatch)
)(Publish);
