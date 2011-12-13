 template <typename T>
    ref struct AutoPtr
    {
        AutoPtr() : m_ptr(0) {}
        AutoPtr(T* ptr) : m_ptr(ptr) {}
        AutoPtr(AutoPtr<T>% right) : m_ptr(right.Release()) {}

        ~AutoPtr()
        {
            delete m_ptr;
            m_ptr = 0;
        }
        !AutoPtr()
        {
           // ASSERT(0 == m_ptr);
            delete m_ptr;
        }
        T* operator->()
        {
            if (0 == m_ptr)
            {
                throw gcnew ObjectDisposedException(String::Empty);
            }

            return m_ptr;
        }

        T* GetPointer()
        {
            return m_ptr;
        }
        T& GetRef()
        {
            if (0 == m_ptr)
            {
                throw gcnew ObjectDisposedException(String::Empty);
            }

            return *m_ptr;
        }
        T* Release()
        {
            T* released = m_ptr;
            m_ptr = 0;
            return released;
        }
        void Reset()
        {
            Reset(0);
        }
        void Reset(T* ptr)
        {
            if (ptr != m_ptr)
            {
                delete m_ptr;
                m_ptr = ptr;
            }
        }

    private:
        T* m_ptr;
    };