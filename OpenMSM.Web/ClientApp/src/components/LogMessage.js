import React, { Component } from 'react';
import { connect } from 'react-redux';
import { Card, CardHeader, CardBody, UncontrolledCollapse } from 'reactstrap'; 
import Time from 'react-time';

class LogMessage extends Component {
    render() {
        return (
            <Card className="admin-message">
                <CardHeader id={"message-" + this.props.index}>
                    <h2 className="float-left"><strong>{this.props.data.requestMethod}</strong> {this.props.data.requestURL}</h2> <Time className="float-right" value={this.props.data.respondedOn} format="h:mm:ss A MM/DD"/>
                </CardHeader>
                <CardBody className="row">
                    <UncontrolledCollapse toggler={"#message-" + this.props.index} className="row" style={{ "width": "100%", "marginLeft": "0px" }}>
                        <div className="admin-message-request col-xs-12 col-sm-6">
                            <h4>Request</h4>
                            <div><strong>IP: </strong>{this.props.data.requestIP}</div>
                            {
                                this.props.data.requestBody.length > 0 ? (
                                    <div>
                                        <strong>Request Body:</strong>
                                        <pre className="json-body">{JSON.stringify(JSON.parse(this.props.data.requestBody), undefined, 2)}</pre>
                                    </div>
                                ) : ('')
                            }
                        </div>
                        <div className="admin-message-response col-xs-12 col-sm-6">
                            <h4>Response</h4>
                            <div><strong>Status Code: </strong>{this.props.data.responseStatus}</div>
                            {
                                this.props.data.responseBody.length > 0 ? (
                                    <div>
                                        <strong>Response Body:</strong>
                                        <pre className="json-body">{JSON.stringify(JSON.parse(this.props.data.responseBody), undefined, 2)}</pre>
                                    </div>
                                ) : ('')
                            }
                        </div>
                    </UncontrolledCollapse>
                </CardBody>
            </Card>
        );
    }
}

export default connect()(LogMessage);
