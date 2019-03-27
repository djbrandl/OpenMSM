import React, { Component } from 'react';
import { connect } from 'react-redux';
import { bindActionCreators } from 'redux';
import { actionCreators } from '../store/HeaderLogging';
import { TabContent, TabPane, Nav, NavItem, NavLink, Row, Col, ListGroup, ListGroupItem, Card, CardTitle } from 'reactstrap';
import classnames from 'classnames';

class HeaderLogging extends Component {
    render() {
        return (
            <div id="header-logging" className={classnames({ "showSlideOut": this.props.slideOut })}>
                <div className="slideOutTab">
                    <div>
                        <p onClick={() => this.props.toggleSlideOut()} style={{ cursor: 'pointer' }}>Logging Information</p>
                    </div>
                </div>
                <div className="modal-content">
                    <div className="modal-header">
                        <Nav tabs>
                            <NavItem>
                                <NavLink className={classnames({ active: this.props.activeHeaderTab === 'LastCall' })} onClick={() => { this.props.setActiveTab('LastCall'); }} href='#'>Last API Call</NavLink>
                            </NavItem>
                            <NavItem>
                                <NavLink className={classnames({ active: this.props.activeHeaderTab === 'Messages' })} onClick={() => { this.props.setActiveTab('Messages') }} href='#'>Server response messages</NavLink>
                            </NavItem>
                        </Nav>
                        
                    </div>
                    <div className="modal-body">
                        <TabContent activeTab={this.props.activeHeaderTab}>
                            <TabPane tabId="LastCall">
                                <Row>
                                    <Col sm="6">
                                        <Card body>
                                            <CardTitle>{this.props.lastApiCall.url}</CardTitle>
                                            <pre>
                                                {JSON.stringify(this.props.lastApiCall.details, undefined, 2)}
                                            </pre>
                                        </Card>
                                    </Col>
                                    <Col sm="6">
                                        <Card body>
                                            <CardTitle>{this.props.lastApiCall.url ? "Response" : ""}</CardTitle>
                                            <pre>
                                                {JSON.stringify(this.props.lastApiCall.response, undefined, 2)}
                                            </pre>
                                        </Card>
                                    </Col>
                                </Row>
                            </TabPane>
                            <TabPane tabId="Messages">
                                <Row>
                                    <Col sm="12">
                                        <br />
                                        <ListGroup>
                                            {this.props.messages.map((message, index) => (
                                                <ListGroupItem key={index}>{message}</ListGroupItem>
                                            ))}
                                        </ListGroup>
                                    </Col>
                                </Row>
                            </TabPane>
                        </TabContent>
                    </div>
                    <div className="modal-footer"> </div>
                </div>
            </div >
        );
    }
}

export default connect(
    state => state.headerLogging,
    dispatch => bindActionCreators(actionCreators, dispatch)
)(HeaderLogging);

/*
<div id="header-logging">
<Nav tabs>
    <NavItem>
        <NavLink className={classnames({ active: this.props.activeHeaderTab === 'LastCall' })} onClick={() => { this.props.setActiveTab('LastCall'); }} href='#'>Last API Call</NavLink>
    </NavItem>
    <NavItem>
        <NavLink className={classnames({ active: this.props.activeHeaderTab === 'Messages' })} onClick={() => { this.props.setActiveTab('Messages') }} href='#'>Server response messages</NavLink>
    </NavItem>
</Nav>
<TabContent activeTab={this.props.activeHeaderTab}>
    <TabPane tabId="LastCall">
        <Row>
            <Col sm="6">
                <Card body>
                    <CardTitle>{this.props.lastApiCall.url}</CardTitle>
                    <pre>
                        {JSON.stringify(this.props.lastApiCall.details, undefined, 2)}
                    </pre>
                </Card>
            </Col>
            <Col sm="6">
                <Card body>
                    <CardTitle>Response</CardTitle>
                    <pre>
                        {JSON.stringify(this.props.lastApiCall.response, undefined, 2)}
                    </pre>
                </Card>
            </Col>
        </Row>
    </TabPane>
    <TabPane tabId="Messages">
        <Row>
            <Col sm="12">
                <br />
                <ListGroup>
                    {this.props.messages.map((message, index) => (
                        <ListGroupItem key={index}>{message}</ListGroupItem>
                    ))}
                </ListGroup>
            </Col>
        </Row>
    </TabPane>
</TabContent>
</div >
*/