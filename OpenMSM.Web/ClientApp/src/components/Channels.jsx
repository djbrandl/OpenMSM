import React, { Component } from 'react';
import { bindActionCreators } from 'redux';
import { connect } from 'react-redux';
import { actionCreators } from '../store/Channels';
import { Jumbotron, Collapse, Card, CardTitle, TabContent, TabPane, Nav, NavItem, NavLink, Row, Col, FormGroup, FormText, Label, Input, Button, InputGroup, InputGroupAddon } from 'reactstrap';
import { Formik, Form, Field, FieldArray, ErrorMessage } from 'formik';
import classnames from 'classnames';

class FetchData extends Component {
    componentDidMount() {
        this.ensureDataFetched();
    }
    ensureDataFetched() {
        this.props.requestChannels();
    }
    render() {
        return (
            <div>
                <Jumbotron>
                    <h1 className="display-3">Channels</h1>
                    <p className="lead">This component demonstrates functionality of the Channel REST API.</p>
                    <hr className="my-2" />
                    <p>To see the details of the web calls performed, click the button below to show/hide the API call details.</p>
                    <Button onClick={this.props.toggleApi}>Toggle API details</Button>
                    <Collapse isOpen={this.props.showApi}>
                        <Row>
                            <Col sm="6">
                                <Card body>
                                    <CardTitle>{this.props.lastApiCallUrl}</CardTitle>
                                    <pre>
                                        {JSON.stringify(this.props.lastApiCallDetails, undefined, 2)}
                                    </pre>
                                </Card>
                            </Col>

                            <Col sm="6">
                                <Card body>
                                    <CardTitle>Response</CardTitle>
                                    <pre>
                                        {JSON.stringify(this.props.lastApiResponse, undefined, 2)}
                                    </pre>
                                </Card>
                            </Col>
                        </Row>
                    </Collapse>
                </Jumbotron>

                <Nav tabs>
                    <NavItem>
                        <NavLink className={classnames({ active: this.props.activeTab === 'Get' })} onClick={() => { this.props.setActiveTab('Get'); }} href='#'>Get Channels</NavLink>
                    </NavItem>
                    <NavItem>
                        <NavLink className={classnames({ active: this.props.activeTab === 'Create' })} onClick={() => { this.props.setActiveTab('Create') }} href='#'>Create Channel</NavLink>
                    </NavItem>
                    <NavItem>
                        <NavLink className={classnames({ active: this.props.activeTab === 'Add' })} onClick={() => { this.props.setActiveTab('Add') }} href='#'>Add Security Tokens</NavLink>
                    </NavItem>
                    <NavItem>
                        <NavLink className={classnames({ active: this.props.activeTab === 'Remove' })} onClick={() => { this.props.setActiveTab('Remove') }} href='#'>Remove Security Tokens</NavLink>
                    </NavItem>
                    <NavItem>
                        <NavLink className={classnames({ active: this.props.activeTab === 'Delete' })} onClick={() => { this.props.setActiveTab('Delete') }} href='#'>Delete Channel</NavLink>
                    </NavItem>
                </Nav>
                <TabContent activeTab={this.props.activeTab}>
                    <TabPane tabId="Get">
                        <Row>
                            <Col sm="12">
                                <br />
                                {renderGetChannels(this.props)}
                            </Col>
                        </Row>
                    </TabPane>
                    <TabPane tabId="Create">
                        <Row>
                            <Col sm="12">
                                <br />
                                {renderCreateForm(this.props)}
                            </Col>
                        </Row>
                    </TabPane>
                    <TabPane tabId="Add">
                        <Row>
                            <Col sm="12">
                                <br />
                                {renderAddTokens(this.props)}
                            </Col>
                        </Row>
                    </TabPane>
                    <TabPane tabId="Remove">
                        <Row>
                            <Col sm="12">
                                <br />
                                {renderRemoveTokens(this.props)}
                            </Col>
                        </Row>
                    </TabPane>
                    <TabPane tabId="Delete">
                        <Row>
                            <Col sm="12">
                                <br />
                                {renderDeleteChannel(this.props)}
                            </Col>
                        </Row>
                    </TabPane>
                </TabContent>
            </div>
        );
    }
}

function renderCreateForm(props) {
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

function renderAddTokens(props) {
    return (
        <Formik
            initialValues={{ accessToken: '', channelUri: '', securityTokens: [] }}
            onSubmit={(values, { setSubmitting, resetForm }) => {
                props.addSecurityTokens({ data: values, setFinished: () => resetForm() });
            }}>
            {({ values, isSubmitting }) => (
                <Form>
                    <FieldArray
                        name="securityTokens"
                        render={arrayHelpers => (
                            <div>
                                <FormGroup row>
                                    <Label for="channelUri" sm={2}>URI</Label>
                                    <Col sm={10}>
                                        <Field className="form-control" type="text" name="channelUri" />
                                        <ErrorMessage name="channelUri" component="div" />
                                    </Col>
                                </FormGroup>
                                <FormGroup row>
                                    <Label for="accessToken" sm={2}>Access Token</Label>
                                    <Col sm={10}>
                                        <Field className="form-control" type="password" name="accessToken" />
                                    </Col>
                                </FormGroup>
                                {!values.securityTokens || values.securityTokens.length === 0 ?
                                    <FormGroup row>
                                        <Button onClick={() => arrayHelpers.push({ token: '' })}>Add Token to List</Button>
                                    </FormGroup>
                                    : ('')}
                                {values.securityTokens.map((securityToken, index) => (
                                    <FormGroup row key={index} >
                                        <InputGroup size="lg">
                                            <Field className='form-control' name={`securityTokens[${index}].token`} />
                                            <InputGroupAddon addonType="append"><Button onClick={() => arrayHelpers.push({ token: '' })}>&nbsp;+&nbsp;</Button></InputGroupAddon>
                                            <InputGroupAddon addonType="append"><Button onClick={() => arrayHelpers.remove(index)}>&nbsp;-&nbsp;</Button></InputGroupAddon>
                                        </InputGroup>
                                    </FormGroup>
                                ))}
                                <br />
                                {values.securityTokens && values.securityTokens.length > 0 ? (
                                    <Button type="submit" disabled={isSubmitting}>Submit</Button>
                                ) : ('')}
                            </div>
                        )}
                    />
                </Form>
            )}
        </Formik >
    )
}

function renderRemoveTokens(props) {
    return (
        <Formik
            initialValues={{ accessToken: '', channelUri: '', securityTokens: [] }}
            onSubmit={(values, { setSubmitting, resetForm }) => {
                props.removeSecurityTokens({ data: values, setFinished: () => resetForm() });
            }}>
            {({ values, isSubmitting }) => (
                <Form>
                    <FieldArray
                        name="securityTokens"
                        render={arrayHelpers => (
                            <div>
                                <FormGroup row>
                                    <Label for="channelUri" sm={2}>URI</Label>
                                    <Col sm={10}>
                                        <Field className="form-control" type="text" name="channelUri" />
                                        <ErrorMessage name="channelUri" component="div" />
                                    </Col>
                                </FormGroup>
                                <FormGroup row>
                                    <Label for="accessToken" sm={2}>Access Token</Label>
                                    <Col sm={10}>
                                        <Field className="form-control" type="password" name="accessToken" />
                                    </Col>
                                </FormGroup>
                                {!values.securityTokens || values.securityTokens.length === 0 ?
                                    <FormGroup row>
                                        <Button onClick={() => arrayHelpers.push({ token: '' })}>Add Token to List</Button>
                                    </FormGroup>
                                    : ('')}
                                {values.securityTokens.map((securityToken, index) => (
                                    <FormGroup row key={index} >
                                        <InputGroup size="lg">
                                            <Field className='form-control' name={`securityTokens[${index}].token`} />
                                            <InputGroupAddon addonType="append"><Button onClick={() => arrayHelpers.push({ token: '' })}>&nbsp;+&nbsp;</Button></InputGroupAddon>
                                            <InputGroupAddon addonType="append"><Button onClick={() => arrayHelpers.remove(index)}>&nbsp;-&nbsp;</Button></InputGroupAddon>
                                        </InputGroup>
                                    </FormGroup>
                                ))}
                                <br />
                                {values.securityTokens && values.securityTokens.length > 0 ? (
                                    <Button type="submit" disabled={isSubmitting}>Submit</Button>
                                ) : ('')}
                            </div>
                        )}
                    />
                </Form>
            )}
        </Formik >
    )
}

function renderDeleteChannel(props) {
    return (
        <Formik
            initialValues={{ accessToken: '', channelUri: '' }}
            onSubmit={(values, { setSubmitting, resetForm }) => {
                props.deleteChannel({ data: values, setFinished: () => resetForm() });
            }}>
            {({ isSubmitting }) => (
                <Form>

                    <FormGroup row>
                        <Label for="channelUri" sm={2}>URI</Label>
                        <Col sm={10}>
                            <Field className="form-control" type="text" name="channelUri" />
                            <ErrorMessage name="channelUri" component="div" />
                        </Col>
                    </FormGroup>
                    <FormGroup row>
                        <Label for="accessToken" sm={2}>Access Token</Label>
                        <Col sm={10}>
                            <Field className="form-control" type="password" name="accessToken" />
                        </Col>
                    </FormGroup>

                    <Button type="submit" disabled={isSubmitting}>Submit</Button>
                </Form>
            )}
        </Formik >
    )
}

function renderGetChannels(props) {
    return (
        <Row>
            <Col sm="12">
                <Form onSubmit={(e) => { props.setAccessToken(e); }}>
                    <FormGroup row>
                        <Label for="token" sm={3}>Set Access Token</Label>
                        <Col sm={7}>
                            <Input type="text" name="token" id="token" placeholder="Access Token" />
                        </Col>
                        <Button sm="2">Set</Button>
                    </FormGroup>
                </Form>
                <table className='table table-striped'>
                    <thead>
                        <tr>
                            <th>URI</th>
                            <th>Type</th>
                            <th>Description</th>
                        </tr>
                    </thead>
                    <tbody>
                        {props.channels.map(channel =>
                            <tr key={channel.uri}>
                                <td>{channel.uri}</td>
                                <td>{channel.type}</td>
                                <td>{channel.description}</td>
                            </tr>
                        )}
                    </tbody>
                </table>
            </Col>
        </Row>
    );
}

export default connect(
    state => state.channels,
    dispatch => bindActionCreators(actionCreators, dispatch)
)(FetchData);
