import { configureStore } from '@reduxjs/toolkit';
import authReducer from './slices/authSlice';

// 创建Redux存储
const store = configureStore({
  reducer: {
    auth: authReducer,
    // 未来可以添加更多reducer
  },
  middleware: (getDefaultMiddleware) =>
    getDefaultMiddleware({
      serializableCheck: {
        // 忽略非序列化值，如日期对象等
        ignoredActions: ['auth/login/fulfilled', 'auth/fetchCurrentUser/fulfilled'],
      },
    }),
});

// 导出存储和类型
export type RootState = ReturnType<typeof store.getState>;
export type AppDispatch = typeof store.dispatch;
export default store;