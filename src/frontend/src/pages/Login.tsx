import React, { useEffect } from 'react';
import { AppDispatch } from 'redux/store';
import { useDispatch, useSelector } from 'react-redux';
import { useNavigate } from 'react-router-dom';
import {
  Form,
  Input,
  Button,
  Card,
  Typography,
  message,
  Spin,
  Layout
} from 'antd';
import { UserOutlined, LockOutlined } from '@ant-design/icons';
import { login, clearError } from '../redux/slices/authSlice';
import { RootState } from '../redux/store';

const { Title } = Typography;
const { Content } = Layout;

const Login: React.FC = () => {
  const [form] = Form.useForm();
  const dispatch = useDispatch<AppDispatch>();
  const navigate = useNavigate();
  const { loading, error, isAuthenticated } = useSelector((state: RootState) => state.auth);
  const [messageApi, contextHolder] = message.useMessage();

  // 如果已认证，重定向到仪表盘
  useEffect(() => {
    if (isAuthenticated) {
      navigate('/dashboard');
    }
  }, [isAuthenticated, navigate]);

  // 显示错误消息
  useEffect(() => {
    if (error) {
      messageApi.error(error);
      // 清除错误状态
      dispatch(clearError());
    }
  }, [error, messageApi, dispatch]);

  // 处理表单提交
  const handleSubmit = async (values: { username: string; password: string }) => {
    try {
      await dispatch(login(values)).unwrap();
      messageApi.success('登录成功');
    } catch (err) {
      // 错误处理已在effect中处理
    }
  };

  return (
    <Layout style={{ minHeight: '100vh' }}>
      {contextHolder}
      <Content style={{ display: 'flex', justifyContent: 'center', alignItems: 'center', background: '#f0f2f5' }}>
        <Card
          style={{ width: 350 }}
          bordered={false}
          className="login-card"
          bodyStyle={{ padding: '32px 40px 48px' }}
        >
          <div style={{ textAlign: 'center', marginBottom: 24 }}>
            <Title level={2}>HHPortal 管理系统</Title>
          </div>
          <Form
            form={form}
            name="login_form"
            layout="vertical"
            onFinish={handleSubmit}
          >
            <Form.Item
              name="username"
              label="用户名"
              rules={[{ required: true, message: '请输入用户名' }]}
              hasFeedback
            >
              <Input
                prefix={<UserOutlined className="site-form-item-icon" />}
                placeholder="请输入用户名"
                autoComplete="username"
              />
            </Form.Item>
            <Form.Item
              name="password"
              label="密码"
              rules={[{ required: true, message: '请输入密码' }]}
              hasFeedback
            >
              <Input
                prefix={<LockOutlined className="site-form-item-icon" />}
                type="password"
                placeholder="请输入密码"
                autoComplete="current-password"
              />
            </Form.Item>
            <Form.Item style={{ marginTop: 24 }}>
              <Button
                type="primary"
                htmlType="submit"
                size="large"
                block
                loading={loading}
              >
                {loading ? <Spin size="small" /> : '登录'}
              </Button>
            </Form.Item>
          </Form>
        </Card>
      </Content>
    </Layout>
  );
};

export default Login;