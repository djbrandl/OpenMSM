import React, { Component } from 'react';
import { bindActionCreators } from 'redux';
import { connect } from 'react-redux';
import { actionCreators } from '../store/Channels';
import { Jumbotron, Collapse, Card, CardTitle, TabContent, TabPane, Nav, NavItem, NavLink, Row, Col, Form, FormGroup, FormText, Label, Input, Button } from 'reactstrap';
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
                            <NavLink className={classnames({ active: this.props.activeTab === 'Get' })} onClick={() => { this.props.setActiveTab('Get'); }}>Get Channels</NavLink>
                        </NavItem>
                        <NavItem>
                            <NavLink className={classnames({ active: this.props.activeTab === 'Create' })} onClick={() => { this.props.setActiveTab('Create') }}>Create Channel</NavLink>
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
                    </TabContent>
                    {renderSelectedChannel(this.props)}
            </div>
                );
            }
        }
        
function renderCreateForm(props) {
    return (
        <Form onSubmit={props.createChannel} name="createChannel">
                    <h2>Create a new channel</h2>
                    <FormGroup row>
                        <Label for="channelUri" sm={2}>URI</Label>
                        <Col sm={10}>
                            <Input type="text" name="uri" id="uri" placeholder="Channel URI" />
                        </Col>
                    </FormGroup>
                    <FormGroup row>
                        <Label for="channelType" sm={2}>Type</Label>
                        <Col sm={10}>
                            <Input type="select" name="type">
                                <option>Publication</option>
                                <option>Request</option>
                            </Input>
                        </Col>
                    </FormGroup>
                    <FormGroup row>
                        <Label for="description" sm={2}>Description</Label>
                        <Col sm={10}>
                            <Input type="textarea" name="description" placeholder="Channel Description" />
                        </Col>
                    </FormGroup>
                    <FormGroup row>
                        <Label for="securityToken" sm={2}>Security Token</Label>
                        <Col sm={10}>
                            <Input type="password" name="securityToken" id="securityToken" placeholder="Security Token" />
                            <FormText color="muted">
                                Current front-end implementation of this demonstration only allows for the user to enter just 1 security token when creating a channel. The underlying API driving the behavior <strong>does</strong> allow for multiple security tokens to be assigned to the channel on creation.
                    </FormText>
                        </Col>
                    </FormGroup>
                    <Button>Submit</Button>
                </Form>
                );
            }
            
function renderSelectedChannel(props) {

                }

                function renderGetChannels(props) {
    return (
        <Row>
                    <Col sm="12">
                        <Form onSubmit={(e) => { props.setAccessToken(e); }}>
                            <FormGroup row>
                                <Label for="token" sm={3}>Set Access Token</Label>
                                <Col sm={7}>
                                    <Input type="password" name="token" id="token" placeholder="Access Token" />
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
