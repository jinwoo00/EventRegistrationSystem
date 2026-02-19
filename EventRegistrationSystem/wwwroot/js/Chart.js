
        document.addEventListener('DOMContentLoaded', function() {
            // ----- 1. Registration Trend (Line) -----
            const trendCtx = document.getElementById('trendChart').getContext('2d');
            const trendLabels = @Html.Raw(Json.Serialize(Model.RegistrationTrend.Select(r => r.Date)));
            const trendData = @Html.Raw(Json.Serialize(Model.RegistrationTrend.Select(r => r.Count)));

            new Chart(trendCtx, {
                type: 'line',
                data: {
                    labels: trendLabels,
                    datasets: [{
                        label: 'Registrations',
                        data: trendData,
                        borderColor: '#6366f1',
                        backgroundColor: 'rgba(99, 102, 241, 0.05)',
                        borderWidth: 2,
                        pointRadius: 3,
                        pointHoverRadius: 5,
                        tension: 0.2,
                        fill: true
                    }]
                },
                options: {
                    responsive: true,
                    maintainAspectRatio: false,
                    plugins: { legend: { display: false } },
                    scales: {
                        y: { beginAtZero: true, grid: { color: '#e2e8f0' } },
                        x: { grid: { display: false } }
                    }
                }
            });

            // ----- 2. Event Attendance Rate (Bar) -----
            const barCtx = document.getElementById('eventBarChart').getContext('2d');
            const eventNames = @Html.Raw(Json.Serialize(Model.EventAttendanceRates.Select(e => e.EventName)));
            const attendancePercents = @Html.Raw(Json.Serialize(Model.EventAttendanceRates.Select(e => e.AttendancePercentage)));

            new Chart(barCtx, {
                type: 'bar',
                data: {
                    labels: eventNames,
                    datasets: [{
                        label: 'Attendance %',
                        data: attendancePercents,
                        backgroundColor: '#818cf8',
                        borderRadius: 6
                    }]
                },
                options: {
                    responsive: true,
                    maintainAspectRatio: false,
                    plugins: {
                        tooltip: {
                            callbacks: {
                                afterLabel: function(context) {
                                    const idx = context.dataIndex;
                                    const event = @Html.Raw(Json.Serialize(Model.EventAttendanceRates));
                                    return `Registered: ${event[idx].registered} | Checked-in: ${event[idx].checkedIn}`;
                                }
                            }
                        }
                    },
                    scales: {
                        y: { beginAtZero: true, max: 100, grid: { color: '#e2e8f0' } }
                    }
                }
            });

            // ----- 3. Attendance Status (Pie) -----
            const pieCtx = document.getElementById('statusPieChart').getContext('2d');
            const statusData = [
                @Model.AttendanceStatus.CheckedIn,
                @Model.AttendanceStatus.NoShow
            ];

            new Chart(pieCtx, {
                type: 'pie',
                data: {
                    labels: ['Checked-in', 'No-show'],
                    datasets: [{
                        data: statusData,
                        backgroundColor: ['#22c55e', '#ef4444'],
                        borderWidth: 0
                    }]
                },
                options: {
                    responsive: true,
                    maintainAspectRatio: false,
                    plugins: {
                        legend: { position: 'bottom' }
                    }
                }
            });
        });