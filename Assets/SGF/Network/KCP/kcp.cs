using System;
using UnityEngine;

namespace SGF.Network.KCP
{
    public class KCP
    {
        public const int IKCP_RTO_NDL = 30;  // no delay min rto 无延迟最小重传时间
        public const int IKCP_RTO_MIN = 100; // normal min rto 正常最小重传时间
        public const int IKCP_RTO_DEF = 200; //默认重传时间
        public const int IKCP_RTO_MAX = 60000; //最大重传时间
        public const int IKCP_CMD_PUSH = 81; // cmd: push data 命令：推送数据
        public const int IKCP_CMD_ACK = 82; // cmd: ack 命令：确认
        public const int IKCP_CMD_WASK = 83; // cmd: window probe (ask) 命令：窗口探测（请求）
        public const int IKCP_CMD_WINS = 84; // cmd: window size (tell) 命令：窗口大小（通知）
        public const int IKCP_ASK_SEND = 1;  // need to send IKCP_CMD_WASK 需要发送窗口探测请求
        public const int IKCP_ASK_TELL = 2;  // need to send IKCP_CMD_WINS 需要发送窗口大小通知
        public const int IKCP_WND_SND = 32; // 发送窗口大小
        public const int IKCP_WND_RCV = 32; // 接收窗口大小
        public const int IKCP_MTU_DEF = 1400; // 默认最大传输单元
        public const int IKCP_ACK_FAST = 3; //快速确认的数量
        public const int IKCP_INTERVAL = 100;// 发送间隔
        public const int IKCP_OVERHEAD = 24;// 协议开销
        public const int IKCP_DEADLINK = 10;//死链检测阈值
        public const int IKCP_THRESH_INIT = 2; //初始化阈值
        public const int IKCP_THRESH_MIN = 2; // 最小阈值
        public const int IKCP_PROBE_INIT = 7000;   // 7 secs to probe window size 探测窗口大小的初始时间
        public const int IKCP_PROBE_LIMIT = 120000; // up to 120 secs to probe window 探测窗口大小的最大时间


        // encode 8 bits unsigned int
        public static int ikcp_encode8u(byte[] p, int offset, byte c)
        {
            p[0 + offset] = c;
            return 1;
        }

        // decode 8 bits unsigned int
        public static int ikcp_decode8u(byte[] p, int offset, ref byte c)
        {
            c = p[0 + offset];
            return 1;
        }

        /* encode 16 bits unsigned int (lsb) */
        public static int ikcp_encode16u(byte[] p, int offset, UInt16 w)
        {
            // 右移 ：00010010 00110100 >> 8 = 00000000 00010010
            // (byte) 转换时，转换最低的8位
            p[0 + offset] = (byte)(w >> 0);
            p[1 + offset] = (byte)(w >> 8);
            return 2;
        }

        /* decode 16 bits unsigned int (lsb) */
        public static int ikcp_decode16u(byte[] p, int offset, ref UInt16 c)
        {
            UInt16 result = 0;
            result |= (UInt16)p[0 + offset];
            // 左移 ：00010010 << 8 = 00010010 00000000
            result |= (UInt16)(p[1 + offset] << 8);
            c = result;
            return 2;
        }

        /* encode 32 bits unsigned int (lsb) */
        public static int ikcp_encode32u(byte[] p, int offset, UInt32 l)
        {
            p[0 + offset] = (byte)(l >> 0);
            p[1 + offset] = (byte)(l >> 8);
            p[2 + offset] = (byte)(l >> 16);
            p[3 + offset] = (byte)(l >> 24);
            return 4;
        }

        /* decode 32 bits unsigned int (lsb) */
        public static int ikcp_decode32u(byte[] p, int offset, ref UInt32 c)
        {
            UInt32 result = 0;
            result |= (UInt32)p[0 + offset];
            result |= (UInt32)(p[1 + offset] << 8);
            result |= (UInt32)(p[2 + offset] << 16);
            result |= (UInt32)(p[3 + offset] << 24);
            c = result;
            return 4;
        }

        public static byte[] slice(byte[] p, int start, int stop)
        {
            var bytes = new byte[stop - start];
            Array.Copy(p, start, bytes, 0, bytes.Length);
            return bytes;
        }

        public static T[] slice<T>(T[] p, int start, int stop)
        {
            var arr = new T[stop - start];
            var index = 0;
            for (var i = start; i < stop; i++)
            {
                arr[index] = p[i];
                index++;
            }

            return arr;
        }

        public static byte[] append(byte[] p, byte c)
        {
            var bytes = new byte[p.Length + 1];
            Array.Copy(p, bytes, p.Length);
            bytes[p.Length] = c;
            return bytes;
        }

        public static T[] append<T>(T[] p, T c)
        {
            var arr = new T[p.Length + 1];
            for (var i = 0; i < p.Length; i++)
                arr[i] = p[i];
            arr[p.Length] = c;
            return arr;
        }

        public static T[] append<T>(T[] p, T[] cs)
        {
            var arr = new T[p.Length + cs.Length];
            for (var i = 0; i < p.Length; i++)
                arr[i] = p[i];
            for (var i = 0; i < cs.Length; i++)
                arr[p.Length + i] = cs[i];
            return arr;
        }

        static UInt32 _imin_(UInt32 a, UInt32 b)
        {
            return a <= b ? a : b;
        }

        static UInt32 _imax_(UInt32 a, UInt32 b)
        {
            return a >= b ? a : b;
        }

        static UInt32 _ibound_(UInt32 lower, UInt32 middle, UInt32 upper)
        {
            return _imin_(_imax_(lower, middle), upper);
        }

        static Int32 _itimediff(UInt32 later, UInt32 earlier)
        {
            return ((Int32)(later - earlier));
        }

        // KCP Segment Definition
        internal class Segment
        {
            internal UInt32 conv = 0;
            internal UInt32 cmd = 0;
            internal UInt32 frg = 0;
            internal UInt32 wnd = 0;
            internal UInt32 ts = 0;
            internal UInt32 sn = 0;
            internal UInt32 una = 0;
            internal UInt32 resendts = 0;
            internal UInt32 rto = 0;
            internal UInt32 fastack = 0;
            internal UInt32 xmit = 0;
            internal byte[] data;

            internal Segment(int size)
            {
                this.data = new byte[size];
            }

            // encode a segment into buffer
            internal int encode(byte[] ptr, int offset)
            {

                var offset_ = offset;

                offset += ikcp_encode32u(ptr, offset, conv);
                offset += ikcp_encode8u(ptr, offset, (byte)cmd);
                offset += ikcp_encode8u(ptr, offset, (byte)frg);
                offset += ikcp_encode16u(ptr, offset, (UInt16)wnd);
                offset += ikcp_encode32u(ptr, offset, ts);
                offset += ikcp_encode32u(ptr, offset, sn);
                offset += ikcp_encode32u(ptr, offset, una);
                offset += ikcp_encode32u(ptr, offset, (UInt32)data.Length);

                return offset - offset_;
            }
        }

        // kcp members.
        UInt32 conv; UInt32 mtu; UInt32 mss; UInt32 state;
        UInt32 snd_una; // snd_una 表示发送缓冲区中已经发送但尚未被接收方确认的第一个数据包
        UInt32 snd_nxt; // snd_nxt 表示下一个即将发送的数据包的序列号。它指向发送缓冲区中下一个待发送的数据包的序列号
        UInt32 rcv_nxt; // rcv_nxt表示下一个想要收到的数据包的序列号

        UInt32 ts_recent; UInt32 ts_lastack; UInt32 ssthresh;
        UInt32 rx_rttval; UInt32 rx_srtt; UInt32 rx_rto; UInt32 rx_minrto;
        UInt32 snd_wnd; UInt32 rcv_wnd; UInt32 rmt_wnd; UInt32 cwnd; UInt32 probe;
        UInt32 current; UInt32 interval; UInt32 ts_flush; UInt32 xmit;
        UInt32 nodelay; UInt32 updated;
        UInt32 ts_probe; UInt32 probe_wait;
        UInt32 dead_link; UInt32 incr;

        Segment[] snd_queue = new Segment[0];
        Segment[] rcv_queue = new Segment[0];
        Segment[] snd_buf = new Segment[0];
        Segment[] rcv_buf = new Segment[0];

        UInt32[] acklist = new UInt32[0];

        byte[] buffer;
        Int32 fastresend;
        Int32 nocwnd;
        Int32 logmask;
        // buffer, size
        Action<byte[], int> output;

        // create a new kcp control object, 'conv' must equal in two endpoint
        // from the same connection.
        public KCP(UInt32 conv_, Action<byte[], int> output_)
        {
            conv = conv_;
            snd_wnd = IKCP_WND_SND;
            rcv_wnd = IKCP_WND_RCV;
            rmt_wnd = IKCP_WND_RCV;
            mtu = IKCP_MTU_DEF;
            mss = mtu - IKCP_OVERHEAD;

            rx_rto = IKCP_RTO_DEF;
            rx_minrto = IKCP_RTO_MIN;
            interval = IKCP_INTERVAL;
            ts_flush = IKCP_INTERVAL;
            ssthresh = IKCP_THRESH_INIT;
            dead_link = IKCP_DEADLINK;
            buffer = new byte[(mtu + IKCP_OVERHEAD) * 3];
            output = output_;
        }

        public void Dispose()
        {
            output = null;
        }

        // check the size of next message in the recv queue
        //检查消息队列中是否有一条完整的消息，并返回其长度
        public int PeekSize()
        {
    
            if (0 == rcv_queue.Length) return -1;// 如果接收队列为空，返回 -1

            var seq = rcv_queue[0];// 获取接收队列中的第一个消息

            if (0 == seq.frg) return seq.data.Length;// 如果消息不分片，则直接返回其长度

            if (rcv_queue.Length < seq.frg + 1) return -1;// 如果队列中的分片数量不足,需要等待继续接收数据，返回 -1

            //走到这代表消息队列中具有一条完整的消息
            int length = 0;
            foreach (var item in rcv_queue)
            {
                length += item.data.Length;// 累加每个消息的长度
                if (0 == item.frg)
                    break;
            }

            return length;
        }

        //当数据从网络接收时，它首先被存储在 接收缓冲区 中。
        //从接收缓冲区，数据会进行校验处理并按顺序移动到 接收队列 中。
        //最后，应用程序将 接收队列 中的整理好的数据 拷贝到用户缓冲区。

        // user/upper level recv: returns size, returns below zero for EAGAIN
        // 将接收队列中已经有序的数据段 移动到用户缓冲区，然后清理接收队列和接收缓存区.
        public int Recv(byte[] buffer)
        {
            // 如果接收队列为空，返回 -1
            if (0 == rcv_queue.Length) return -1;
            // 如果 PeekSize 返回负值，表示需要更多数据，返回 -2
            var peekSize = PeekSize();
            if (0 > peekSize) return -2;
            // 如果可接收的大小超过了缓冲区长度，返回 -3
            if (peekSize > buffer.Length) return -3;

            var fast_recover = false;// 检查是否需要快速恢复
            if (rcv_queue.Length >= rcv_wnd) fast_recover = true;

            // 合并片段，准备将数据队列中的数据拷贝到用户缓冲区
            var count = 0;
            var n = 0;
            // 循环将多个数据段 拷贝到 用户缓存区， 合并为一个完整的消息
            foreach (var seg in rcv_queue)
            {
                Array.Copy(seg.data, 0, buffer, n, seg.data.Length);
                n += seg.data.Length;
                count++;
                if (0 == seg.frg) break;
            }

            if (0 < count)
            {
                // 移除接收队列中，已经转移到用户缓存区的数据段
                rcv_queue = slice<Segment>(rcv_queue, count, rcv_queue.Length);
            }

            // move available data from rcv_buf -> rcv_queue
            // 从接收缓冲区移动合法数据到接收队列
            count = 0;
            foreach (var seg in rcv_buf)
            {
                if (seg.sn == rcv_nxt && rcv_queue.Length < rcv_wnd)
                {
                    rcv_queue = append<Segment>(rcv_queue, seg);// 将新的片段添加到接收队列
                    rcv_nxt++;// 更新下一个期望序列号
                    count++;
                }
                else
                {
                    break;
                }
            }
            // 从接收缓冲区中移除已处理的片段
            if (0 < count) rcv_buf = slice<Segment>(rcv_buf, count, rcv_buf.Length);

            // fast recover
            if (rcv_queue.Length < rcv_wnd && fast_recover)
            {
                // ready to send back IKCP_CMD_WINS in ikcp_flush
                // tell remote my window size
                probe |= IKCP_ASK_TELL;// 准备发送窗口大小通知
            }

            return n;// 返回实际接收到的字节数
        }

        // user/upper level send, returns below zero for error
        public int Send(byte[] buffer, int bufferSize)
        {

            if (0 == bufferSize) return -1;

            var count = 0;
            // 计算需要发送的片段数量 ,bufferSize 指的是要发送的数据大小
            if (bufferSize < mss) //如果缓冲区大小小于最大分段大小（mss）
                count = 1;// 只需发送一个片段
            else
                count = (int)(bufferSize + mss - 1) / (int)mss;// 计算片段数量，向上取整

            //在许多网络协议中，数据片段的长度通常是由一个字节（8 位）表示的。
            //一个字节的最大值是 255，因此如果计算出的片段数量超过 255，就无法在一个字节中表示这个数量。
            if (255 < count) return -2;

            if (0 == count) count = 1;

            var offset = 0;

            for (var i = 0; i < count; i++)
            {
                var size = 0;
                if (bufferSize > mss)
                    size = (int)mss;
                else
                    size = bufferSize - offset;

                var seg = new Segment(size);
                Array.Copy(buffer, offset, seg.data, 0, size); // 复制数据到新片段
                offset += size;
                seg.frg = (UInt32)(count - i - 1); // 设置片段的分段标志，指示还有多少片段未发送
                snd_queue = append<Segment>(snd_queue, seg);
            }

            return 0;
        }

        // update ack.
        void update_ack(Int32 rtt)
        {
            //rx_srtt 是一个平滑RTT，用于计算近期的平均RTT值。用于计算重传超时（RTO）
            // 如果此时的平滑 RTT（rx_srtt）为 0，表示其尚未初始化
            //rx_rttval是 RTT 变动值，通常称为 RTT 变动，是指网络延迟（RTT）的波动程度。它用于描述 RTT 的不稳定性
            if (0 == rx_srtt)
            {
                rx_srtt = (UInt32)rtt; // 初始化平滑 RTT 为当前 RTT 值
                rx_rttval = (UInt32)rtt / 2;
            }
            else
            {
                Int32 delta = (Int32)((UInt32)rtt - rx_srtt);// 计算当前 RTT 与平滑 RTT 之间的差值
                if (0 > delta) delta = -delta;

                rx_rttval = (3 * rx_rttval + (uint)delta) / 4; // 更新 RTT 变动值
                rx_srtt = (UInt32)((7 * rx_srtt + rtt) / 8); // 更新平滑 RTT，
                if (rx_srtt < 1) rx_srtt = 1;
            }
            // 计算重传超时（RTO），RTO = 平滑 RTT + 最大值(1, 4 * RTT 变动值)
            var rto = (int)(rx_srtt + _imax_(1, 4 * rx_rttval));
            rx_rto = _ibound_(rx_minrto, (UInt32)rto, IKCP_RTO_MAX);// 确保 RTO 在最小值和最大值之间
        }

        // 更新未确认的序列号（snd_una）
        void shrink_buf()
        {
            // 如果发送缓冲区不为空，将未确认序列号(指的是当前发出的包中，第一个还未收到确认的号)（snd_una）设置为发送缓冲区中第一个数据包的序列号
            if (snd_buf.Length > 0)
                snd_una = snd_buf[0].sn;
            else
                snd_una = snd_nxt;
        }

        // 处理收到的ACK确认号
        void parse_ack(UInt32 sn)
        {
            // 确认号无效
            if (_itimediff(sn, snd_una) < 0 || _itimediff(sn, snd_nxt) >= 0) return;

            var index = 0;
            // 遍历发送缓冲区，从发送缓冲区中删除已确认的数据段
            foreach (var seg in snd_buf)
            {
                if (sn == seg.sn)
                {
                    // 把收到确认的数据从发送缓冲区中截掉
                    snd_buf = append<Segment>(slice<Segment>(snd_buf, 0, index), slice<Segment>(snd_buf, index + 1, snd_buf.Length));
                    break;
                }
                else  
                {
                    //发送缓冲区中的数据段应该遵循先发先收的原则。也就是说，先发送的数据段应该先接收到确认（ACK）
                    // 如果序列号不匹配，那么前面被跳过去的数据段都要 增加快速确认计数
                    seg.fastack++;
                }

                index++;
            }
        }

        // 处理收到的未确认的序列号（UNA），更新发送缓冲区
        void parse_una(UInt32 una)
        {
            var count = 0;
            // 例如收到的UNA为5，那么表示序列号5之前的数据段(1~4)，都已经被对方收到了，那么可以把他们从发送缓冲区中删去了
            foreach (var seg in snd_buf)
            {
                if (_itimediff(una, seg.sn) > 0)
                    count++;
                else
                    break;
            }

            if (0 < count) snd_buf = slice<Segment>(snd_buf, count, snd_buf.Length);
        }

        // 将收到的数据信息包 的序列号 和收到时间存起来
        void ack_push(UInt32 sn, UInt32 ts)
        {
            acklist = append<UInt32>(acklist, new UInt32[2] { sn, ts });
        }

        void ack_get(int p, ref UInt32 sn, ref UInt32 ts)
        {
            sn = acklist[p * 2 + 0];
            ts = acklist[p * 2 + 1];
        }

        // 处理接收到的数据段，维护接收缓冲区和接收队列
        void parse_data(Segment newseg)
        {
            var sn = newseg.sn;
            // 检查序列号是否在有效范围内
            // 如果序列号超出接收窗口或小于下一个期望的序列号，直接返回
            if (_itimediff(sn, rcv_nxt + rcv_wnd) >= 0 || _itimediff(sn, rcv_nxt) < 0) return;
            
            var n = rcv_buf.Length - 1;
            var after_idx = -1; // 初始化插入位置索引
            var repeat = false;// 标记新段是否重复
            // 从后向前遍历接收缓冲区
            for (var i = n; i >= 0; i--)
            {
                var seg = rcv_buf[i];
                // 检查是否为重复段
                if (seg.sn == sn)
                {
                    //当前收到的数据，已经存在于接受缓冲区,是重复数据
                    repeat = true;
                    break;
                }
                // 如果新段的序列号大于当前段，即找到了插入位置
                if (_itimediff(sn, seg.sn) > 0)
                {
                    after_idx = i;
                    break;
                }
            }

            // 如果新段不是重复的
            if (!repeat)
            {
                // 如果插入索引为 -1，表示新段应该插入在缓冲区的最前面
                if (after_idx == -1)
                    rcv_buf = append<Segment>(new Segment[1] { newseg }, rcv_buf);
                else
                    // 在插入位置后插入新段
                    rcv_buf = append<Segment>(slice<Segment>(rcv_buf, 0, after_idx + 1), append<Segment>(new Segment[1] { newseg }, slice<Segment>(rcv_buf, after_idx + 1, rcv_buf.Length)));
            }

            // move available data from rcv_buf -> rcv_queue
            // 将合法的数据段，从接收缓冲区移动到接收队列
            var count = 0;
            foreach (var seg in rcv_buf)
            {
                if (seg.sn == rcv_nxt && rcv_queue.Length < rcv_wnd)
                {
                    rcv_queue = append<Segment>(rcv_queue, seg);
                    rcv_nxt++;
                    count++;
                }
                else
                {
                    break;
                }
            }

            if (0 < count)
            {
                rcv_buf = slice<Segment>(rcv_buf, count, rcv_buf.Length);
            }
        }

        // when you received a low level packet (eg. UDP packet), call it
        public int Input(byte[] data)
        {

            var s_una = snd_una;
            if (data.Length < IKCP_OVERHEAD) return 0;

            var offset = 0;

            while (true)
            {
                UInt32 ts = 0;
                UInt32 sn = 0;
                UInt32 length = 0;
                UInt32 una = 0;
                UInt32 conv_ = 0;

                UInt16 wnd = 0;

                byte cmd = 0;
                byte frg = 0;

                if (data.Length - offset < IKCP_OVERHEAD) break;

                offset += ikcp_decode32u(data, offset, ref conv_);

                if (conv != conv_) return -1;

                offset += ikcp_decode8u(data, offset, ref cmd);
                offset += ikcp_decode8u(data, offset, ref frg);
                offset += ikcp_decode16u(data, offset, ref wnd);
                offset += ikcp_decode32u(data, offset, ref ts);
                offset += ikcp_decode32u(data, offset, ref sn);
                offset += ikcp_decode32u(data, offset, ref una);
                offset += ikcp_decode32u(data, offset, ref length);

                if (data.Length - offset < length) return -2;

                switch (cmd)
                {
                    case IKCP_CMD_PUSH:
                    case IKCP_CMD_ACK:
                    case IKCP_CMD_WASK:
                    case IKCP_CMD_WINS:
                        break;
                    default:
                        return -3;
                }

                rmt_wnd = (UInt32)wnd;
                parse_una(una);
                shrink_buf();

                if (IKCP_CMD_ACK == cmd)
                {
                    if (_itimediff(current, ts) >= 0)
                    {
                        update_ack(_itimediff(current, ts));
                    }
                    parse_ack(sn);
                    shrink_buf();
                }
                else if (IKCP_CMD_PUSH == cmd)
                {
                    if (_itimediff(sn, rcv_nxt + rcv_wnd) < 0)
                    {
                        ack_push(sn, ts);
                        if (_itimediff(sn, rcv_nxt) >= 0)
                        {
                            var seg = new Segment((int)length);
                            seg.conv = conv_;
                            seg.cmd = (UInt32)cmd;
                            seg.frg = (UInt32)frg;
                            seg.wnd = (UInt32)wnd;
                            seg.ts = ts;
                            seg.sn = sn;
                            seg.una = una;

                            if (length > 0) Array.Copy(data, offset, seg.data, 0, length);

                            parse_data(seg);
                        }
                    }
                }
                else if (IKCP_CMD_WASK == cmd)
                {
                    // ready to send back IKCP_CMD_WINS in Ikcp_flush
                    // tell remote my window size
                    probe |= IKCP_ASK_TELL;
                }
                else if (IKCP_CMD_WINS == cmd)
                {
                    // do nothing
                }
                else
                {
                    return -3;
                }

                offset += (int)length;
            }

            if (_itimediff(snd_una, s_una) > 0)
            {
                if (cwnd < rmt_wnd)
                {
                    var mss_ = mss;
                    if (cwnd < ssthresh)
                    {
                        cwnd++;
                        incr += mss_;
                    }
                    else
                    {
                        if (incr < mss_)
                        {
                            incr = mss_;
                        }
                        incr += (mss_ * mss_) / incr + (mss_ / 16);
                        if ((cwnd + 1) * mss_ <= incr) cwnd++;
                    }
                    if (cwnd > rmt_wnd)
                    {
                        cwnd = rmt_wnd;
                        incr = rmt_wnd * mss_;
                    }
                }
            }

            return 0;
        }

        Int32 wnd_unused()
        {
            if (rcv_queue.Length < rcv_wnd)
                return (Int32)(int)rcv_wnd - rcv_queue.Length;
            return 0;
        }

        // flush pending data
        void flush()
        {
            var current_ = current;
            var buffer_ = buffer;
            var change = 0;
            var lost = 0;

            if (0 == updated) return;

            var seg = new Segment(0);
            seg.conv = conv;
            seg.cmd = IKCP_CMD_ACK;
            seg.wnd = (UInt32)wnd_unused();
            seg.una = rcv_nxt;

            // flush acknowledges
            var count = acklist.Length / 2;
            var offset = 0;
            for (var i = 0; i < count; i++)
            {
                if (offset + IKCP_OVERHEAD > mtu)
                {
                    output(buffer, offset);
                    //Array.Clear(buffer, 0, offset);
                    offset = 0;
                }
                ack_get(i, ref seg.sn, ref seg.ts);
                offset += seg.encode(buffer, offset);
            }
            acklist = new UInt32[0];

            // probe window size (if remote window size equals zero)
            if (0 == rmt_wnd)
            {
                if (0 == probe_wait)
                {
                    probe_wait = IKCP_PROBE_INIT;
                    ts_probe = current + probe_wait;
                }
                else
                {
                    if (_itimediff(current, ts_probe) >= 0)
                    {
                        if (probe_wait < IKCP_PROBE_INIT)
                            probe_wait = IKCP_PROBE_INIT;
                        probe_wait += probe_wait / 2;
                        if (probe_wait > IKCP_PROBE_LIMIT)
                            probe_wait = IKCP_PROBE_LIMIT;
                        ts_probe = current + probe_wait;
                        probe |= IKCP_ASK_SEND;
                    }
                }
            }
            else
            {
                ts_probe = 0;
                probe_wait = 0;
            }

            // flush window probing commands
            if ((probe & IKCP_ASK_SEND) != 0)
            {
                seg.cmd = IKCP_CMD_WASK;
                if (offset + IKCP_OVERHEAD > (int)mtu)
                {
                    output(buffer, offset);
                    //Array.Clear(buffer, 0, offset);
                    offset = 0;
                }
                offset += seg.encode(buffer, offset);
            }

            probe = 0;

            // calculate window size
            var cwnd_ = _imin_(snd_wnd, rmt_wnd);
            if (0 == nocwnd)
                cwnd_ = _imin_(cwnd, cwnd_);

            count = 0;
            for (var k = 0; k < snd_queue.Length; k++)
            {
                if (_itimediff(snd_nxt, snd_una + cwnd_) >= 0) break;

                var newseg = snd_queue[k];
                newseg.conv = conv;
                newseg.cmd = IKCP_CMD_PUSH;
                newseg.wnd = seg.wnd;
                newseg.ts = current_;
                newseg.sn = snd_nxt;
                newseg.una = rcv_nxt;
                newseg.resendts = current_;
                newseg.rto = rx_rto;
                newseg.fastack = 0;
                newseg.xmit = 0;
                snd_buf = append<Segment>(snd_buf, newseg);
                snd_nxt++;
                count++;
            }

            if (0 < count)
            {
                snd_queue = slice<Segment>(snd_queue, count, snd_queue.Length);
            }

            // calculate resent
            var resent = (UInt32)fastresend;
            if (fastresend <= 0) resent = 0xffffffff;
            var rtomin = rx_rto >> 3;
            if (nodelay != 0) rtomin = 0;

            // flush data segments
            foreach (var segment in snd_buf)
            {
                var needsend = false;
                var debug = _itimediff(current_, segment.resendts);
                if (0 == segment.xmit)
                {
                    needsend = true;
                    segment.xmit++;
                    segment.rto = rx_rto;
                    segment.resendts = current_ + segment.rto + rtomin;
                }
                else if (_itimediff(current_, segment.resendts) >= 0)
                {
                    needsend = true;
                    segment.xmit++;
                    xmit++;
                    if (0 == nodelay)
                        segment.rto += rx_rto;
                    else
                        segment.rto += rx_rto / 2;
                    segment.resendts = current_ + segment.rto;
                    lost = 1;
                }
                else if (segment.fastack >= resent)
                {
                    needsend = true;
                    segment.xmit++;
                    segment.fastack = 0;
                    segment.resendts = current_ + segment.rto;
                    change++;
                }

                if (needsend)
                {
                    segment.ts = current_;
                    segment.wnd = seg.wnd;
                    segment.una = rcv_nxt;

                    var need = IKCP_OVERHEAD + segment.data.Length;
                    if (offset + need >= mtu)
                    {
                        output(buffer, offset);
                        //Array.Clear(buffer, 0, offset);
                        offset = 0;
                    }

                    offset += segment.encode(buffer, offset);
                    if (segment.data.Length > 0)
                    {
                        Array.Copy(segment.data, 0, buffer, offset, segment.data.Length);
                        offset += segment.data.Length;
                    }

                    if (segment.xmit >= dead_link)
                    {
                        state = 0;
                    }
                }
            }

            // flash remain segments
            if (offset > 0)
            {
                output(buffer, offset);
                //Array.Clear(buffer, 0, offset);
                offset = 0;
            }

            // update ssthresh
            if (change != 0)
            {
                var inflight = snd_nxt - snd_una;
                ssthresh = inflight / 2;
                if (ssthresh < IKCP_THRESH_MIN)
                    ssthresh = IKCP_THRESH_MIN;
                cwnd = ssthresh + resent;
                incr = cwnd * mss;
            }

            if (lost != 0)
            {
                ssthresh = cwnd / 2;
                if (ssthresh < IKCP_THRESH_MIN)
                    ssthresh = IKCP_THRESH_MIN;
                cwnd = 1;
                incr = mss;
            }

            if (cwnd < 1)
            {
                cwnd = 1;
                incr = mss;
            }
        }

        // update state (call it repeatedly, every 10ms-100ms), or you can ask
        // ikcp_check when to call it again (without ikcp_input/_send calling).
        // 'current' - current timestamp in millisec.
        public void Update(UInt32 current_)
        {

            current = current_;

            if (0 == updated)
            {
                updated = 1;
                ts_flush = current;
            }

            var slap = _itimediff(current, ts_flush);

            if (slap >= 10000 || slap < -10000)
            {
                ts_flush = current;
                slap = 0;
            }

            if (slap >= 0)
            {
                ts_flush += interval;
                if (_itimediff(current, ts_flush) >= 0)
                    ts_flush = current + interval;
                flush();
            }
        }

        // Determine when should you invoke ikcp_update:
        // returns when you should invoke ikcp_update in millisec, if there
        // is no ikcp_input/_send calling. you can call ikcp_update in that
        // time, instead of call update repeatly.
        // Important to reduce unnacessary ikcp_update invoking. use it to
        // schedule ikcp_update (eg. implementing an epoll-like mechanism,
        // or optimize ikcp_update when handling massive kcp connections)
        public UInt32 Check(UInt32 current_)
        {

            if (0 == updated) return current_;

            var ts_flush_ = ts_flush;
            var tm_flush_ = 0x7fffffff;
            var tm_packet = 0x7fffffff;
            var minimal = 0;

            if (_itimediff(current_, ts_flush_) >= 10000 || _itimediff(current_, ts_flush_) < -10000)
            {
                ts_flush_ = current_;
            }

            if (_itimediff(current_, ts_flush_) >= 0) return current_;

            tm_flush_ = (int)_itimediff(ts_flush_, current_);

            foreach (var seg in snd_buf)
            {
                var diff = _itimediff(seg.resendts, current_);
                if (diff <= 0) return current_;
                if (diff < tm_packet) tm_packet = (int)diff;
            }

            minimal = (int)tm_packet;
            if (tm_packet >= tm_flush_) minimal = (int)tm_flush_;
            if (minimal >= interval) minimal = (int)interval;

            return current_ + (UInt32)minimal;
        }

        // change MTU size, default is 1400
        public int SetMtu(Int32 mtu_)
        {
            if (mtu_ < 50 || mtu_ < (Int32)IKCP_OVERHEAD) return -1;

            var buffer_ = new byte[(mtu_ + IKCP_OVERHEAD) * 3];
            if (null == buffer_) return -2;

            mtu = (UInt32)mtu_;
            mss = mtu - IKCP_OVERHEAD;
            buffer = buffer_;
            return 0;
        }

        public int Interval(Int32 interval_)
        {
            if (interval_ > 5000)
            {
                interval_ = 5000;
            }
            else if (interval_ < 10)
            {
                interval_ = 10;
            }
            interval = (UInt32)interval_;
            return 0;
        }

        // fastest: ikcp_nodelay(kcp, 1, 20, 2, 1)
        // nodelay: 0:disable(default), 1:enable
        // interval: internal update timer interval in millisec, default is 100ms
        // resend: 0:disable fast resend(default), 1:enable fast resend
        // nc: 0:normal congestion control(default), 1:disable congestion control
        public int NoDelay(int nodelay_, int interval_, int resend_, int nc_)
        {

            if (nodelay_ > 0)
            {
                nodelay = (UInt32)nodelay_;
                if (nodelay_ != 0)
                    rx_minrto = IKCP_RTO_NDL;
                else
                    rx_minrto = IKCP_RTO_MIN;
            }

            if (interval_ >= 0)
            {
                if (interval_ > 5000)
                {
                    interval_ = 5000;
                }
                else if (interval_ < 10)
                {
                    interval_ = 10;
                }
                interval = (UInt32)interval_;
            }

            if (resend_ >= 0) fastresend = resend_;

            if (nc_ >= 0) nocwnd = nc_;

            return 0;
        }

        // set maximum window size: sndwnd=32, rcvwnd=32 by default
        public int WndSize(int sndwnd, int rcvwnd)
        {
            if (sndwnd > 0)
                snd_wnd = (UInt32)sndwnd;

            if (rcvwnd > 0)
                rcv_wnd = (UInt32)rcvwnd;
            return 0;
        }

        // get how many packet is waiting to be sent
        public int WaitSnd()
        {
            return snd_buf.Length + snd_queue.Length;
        }
    }


}
